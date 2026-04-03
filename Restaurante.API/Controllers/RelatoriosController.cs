using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = IdentityInitializer.AdminRole)]
    public class RelatoriosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("faturamento-por-atendimento")]
        public async Task<IActionResult> FaturamentoPorAtendimento([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            if (fim < inicio)
                return BadRequest(new { message = "Período inválido." });

            var resultado = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Atendimento)
                .Where(p => p.DataHora >= inicio && p.DataHora <= fim)
                .GroupBy(p => p.Atendimento.Tipo)
                .Select(g => new
                {
                    TipoAtendimento = g.Key,
                    QuantidadePedidos = g.Count(),
                    FaturamentoTotal = g.Sum(x => x.ValorTotal)
                })
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("itens-mais-vendidos")]
        public async Task<IActionResult> ItensMaisVendidos([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            if (fim < inicio)
                return BadRequest(new { message = "Período inválido." });

            var itens = await _context.PedidoItens
                .AsNoTracking()
                .Include(pi => pi.Pedido)
                .Include(pi => pi.ItemCardapio)
                .Where(pi => pi.Pedido.DataHora >= inicio && pi.Pedido.DataHora <= fim)
                .GroupBy(pi => new
                {
                    pi.ItemCardapioId,
                    pi.ItemCardapio.Nome,
                    TeveSugestao = pi.PercentualDesconto > 0
                })
                .Select(g => new
                {
                    g.Key.ItemCardapioId,
                    g.Key.Nome,
                    g.Key.TeveSugestao,
                    QuantidadeVendida = g.Sum(x => x.Quantidade),
                    ValorTotal = g.Sum(x => x.PrecoUnitario * x.Quantidade * (1 - x.PercentualDesconto / 100m))
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .ToListAsync();

            return Ok(itens);
        }
    }
}
