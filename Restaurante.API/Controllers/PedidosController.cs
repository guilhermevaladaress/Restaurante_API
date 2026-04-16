using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Pedidos;
using Restaurante.API.Models;
using Restaurante.API.Services;
using System.Security.Claims;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISugestaoChefeService _sugestaoChefeService;

        public PedidosController(
            AppDbContext context,
            ISugestaoChefeService sugestaoChefeService)
        {
            _context = context;
            _sugestaoChefeService = sugestaoChefeService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarPedidoRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var usuarioExiste = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == usuarioId);

            if (!usuarioExiste)
                return Unauthorized(new { message = "Usuário autenticado não encontrado. Faça login novamente." });

            var itemIds = request.Itens.Select(i => i.ItemCardapioId).Distinct().ToList();
            var itensCardapio = await _context.ItensCardapio
                .Where(i => itemIds.Contains(i.Id) && i.Ativo)
                .ToDictionaryAsync(i => i.Id);

            if (itensCardapio.Count != itemIds.Count)
                return BadRequest(new { message = "Um ou mais itens do cardápio são inválidos ou estão inativos." });

            if (itensCardapio.Values.Any(i => i.Periodo != request.Periodo))
                return BadRequest(new { message = "Pedidos devem conter itens do mesmo período selecionado." });

            var agora = DateTime.Now;
            var hoje = DateOnly.FromDateTime(agora);
            var sugestaoHoje = await _sugestaoChefeService.ObterParaDataAsync(hoje, request.Periodo);
            var sugestaoItemId = sugestaoHoje?.ItemCardapioId;
            var percentualDesconto = sugestaoHoje?.PercentualDesconto ?? 0m;

            Atendimento atendimento;
            switch (request.TipoAtendimento)
            {
                case TipoAtendimento.Presencial:
                    if (!request.NumeroMesa.HasValue || request.NumeroMesa.Value <= 0)
                        return BadRequest(new { message = "NumeroMesa é obrigatório para atendimento presencial." });

                    atendimento = new AtendimentoPresencial
                    {
                        DataHora = agora,
                        NumerMesa = request.NumeroMesa.Value
                    };
                    break;

                case TipoAtendimento.DeliveryProprio:
                    if (string.IsNullOrWhiteSpace(request.EnderecoEntrega))
                        return BadRequest(new { message = "EnderecoEntrega é obrigatório para delivery próprio." });

                    atendimento = new AtendimentoDeliveryProprio
                    {
                        DataHora = agora,
                        TaxaEntrega = AtendimentoDeliveryProprio.TaxaFixaEntrega,
                        EnderecoEntrega = request.EnderecoEntrega
                    };
                    break;

                case TipoAtendimento.DeliveryAplicativo:
                    if (string.IsNullOrWhiteSpace(request.NomeAplicativo))
                        return BadRequest(new { message = "NomeAplicativo é obrigatório para delivery por aplicativo." });
                    if (string.IsNullOrWhiteSpace(request.EnderecoEntrega))
                        return BadRequest(new { message = "EnderecoEntrega é obrigatório para delivery por aplicativo." });

                    var nomeAplicativo = request.NomeAplicativo.Trim();
                    if (!nomeAplicativo.Equals("IFood", StringComparison.OrdinalIgnoreCase)
                        && !nomeAplicativo.Equals("Aiqfome", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(new { message = "NomeAplicativo deve ser IFood ou Aiqfome." });
                    }

                    nomeAplicativo = nomeAplicativo.Equals("IFood", StringComparison.OrdinalIgnoreCase)
                        ? "IFood"
                        : "Aiqfome";

                    atendimento = new AtendimentoDeliveryAplicativo
                    {
                        DataHora = agora,
                        NomeAplicativo = nomeAplicativo,
                        EnderecoEntrega = request.EnderecoEntrega
                    };
                    break;

                default:
                    return BadRequest(new { message = "TipoAtendimento inválido." });
            }

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                DataHora = agora,
                Status = StatusPedido.Pendente,
                Atendimento = atendimento
            };

            foreach (var item in request.Itens)
            {
                var itemCardapio = itensCardapio[item.ItemCardapioId];
                var desconto = sugestaoItemId.HasValue && sugestaoItemId.Value == item.ItemCardapioId
                    ? percentualDesconto
                    : 0m;

                pedido.PedidoItens.Add(new PedidoItem
                {
                    ItemCardapioId = item.ItemCardapioId,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = itemCardapio.PrecoBase,
                    PercentualDesconto = desconto
                });
            }

            var subtotalComDesconto = pedido.PedidoItens.Sum(i => i.PrecoFinal);
            var taxa = atendimento.CalcularTaxa(subtotalComDesconto);
            pedido.ValorTotal = subtotalComDesconto + taxa;

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterMeusPedidos), new { }, new
            {
                pedido.Id,
                pedido.DataHora,
                pedido.Status,
                pedido.ValorTotal,
                pedido.AtendimentoId,
                TipoAtendimento = pedido.Atendimento.Tipo
            });
        }

        [HttpGet("meus")]
        public async Task<IActionResult> ObterMeusPedidos()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Atendimento)
                .Include(p => p.PedidoItens)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.DataHora)
                .Select(p => new
                {
                    p.Id,
                    p.DataHora,
                    p.Status,
                    p.ValorTotal,
                    TipoAtendimento = p.Atendimento.Tipo,
                    Itens = p.PedidoItens.Select(i => new
                    {
                        i.ItemCardapioId,
                        i.Quantidade,
                        i.PrecoUnitario,
                        i.PercentualDesconto,
                        i.PrecoFinal
                    })
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Atendimento)
                .Include(p => p.PedidoItens)
                .ThenInclude(pi => pi.ItemCardapio)
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            if (pedido is null)
                return NotFound(new { message = "Pedido não encontrado." });

            return Ok(new
            {
                pedido.Id,
                pedido.DataHora,
                pedido.Status,
                pedido.ValorTotal,
                TipoAtendimento = pedido.Atendimento.Tipo,
                Itens = pedido.PedidoItens.Select(i => new
                {
                    i.ItemCardapioId,
                    NomeItem = i.ItemCardapio.Nome,
                    i.Quantidade,
                    i.PrecoUnitario,
                    i.PercentualDesconto,
                    i.PrecoFinal
                })
            });
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusPedidoRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido is null)
                return NotFound(new { message = "Pedido não encontrado." });

            pedido.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                pedido.Id,
                pedido.Status
            });
        }
    }
}
