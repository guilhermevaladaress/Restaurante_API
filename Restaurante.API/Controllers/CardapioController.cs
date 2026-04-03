using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Cardapio;
using Restaurante.API.Models;
using Restaurante.API.Services;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardapioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISugestaoChefeService _sugestaoChefeService;

        public CardapioController(
            AppDbContext context,
            ISugestaoChefeService sugestaoChefeService)
        {
            _context = context;
            _sugestaoChefeService = sugestaoChefeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCardapioResponseDto>>> Get([FromQuery] PeriodoRefeicao periodo)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var sugestaoHoje = await _sugestaoChefeService.ObterParaDataAsync(hoje, periodo);
            var sugestaoItemId = sugestaoHoje?.ItemCardapioId;
            var percentualDesconto = sugestaoHoje?.PercentualDesconto ?? 0m;

            var itens = await _context.ItensCardapio
                .AsNoTracking()
                .Where(i => i.Ativo && i.Periodo == periodo)
                .Select(i => new ItemCardapioResponseDto
                {
                    Id = i.Id,
                    Nome = i.Nome,
                    Descricao = i.Descricao,
                    PrecoBase = i.PrecoBase,
                    Periodo = i.Periodo,
                    EhSugestaoChefeHoje = sugestaoItemId.HasValue && sugestaoItemId.Value == i.Id,
                    PrecoComDescontoHoje = sugestaoItemId.HasValue && sugestaoItemId.Value == i.Id
                        ? i.PrecoBase * (1 - percentualDesconto / 100m)
                        : i.PrecoBase
                })
                .ToListAsync();

            return Ok(itens);
        }

        [HttpPost]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Criar([FromBody] CriarItemCardapioRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var item = new ItemCardapio
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                PrecoBase = request.PrecoBase,
                Periodo = request.Periodo,
                Ativo = request.Ativo
            };

            _context.ItensCardapio.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { periodo = item.Periodo }, new
            {
                item.Id,
                item.Nome,
                item.Descricao,
                item.PrecoBase,
                item.Periodo,
                item.Ativo
            });
        }

        [HttpPost("{itemId:int}/ingredientes")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> VincularIngredientes(int itemId, [FromBody] VincularIngredientesRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var item = await _context.ItensCardapio.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item is null)
                return NotFound(new { message = "Item do cardápio não encontrado." });

            var ingredienteIds = request.IngredienteIds.Distinct().ToList();
            var ingredientesExistentes = await _context.Ingredientes
                .Where(i => ingredienteIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();

            if (ingredientesExistentes.Count != ingredienteIds.Count)
                return BadRequest(new { message = "Um ou mais ingredientes informados não existem." });

            var relacionamentosExistentes = await _context.ItemCardapioIngredientes
                .Where(ii => ii.ItemCardapioId == itemId && ingredienteIds.Contains(ii.IngredienteId))
                .Select(ii => ii.IngredienteId)
                .ToListAsync();

            var paraAdicionar = ingredienteIds.Except(relacionamentosExistentes)
                .Select(ingredienteId => new ItemCardapioIngrediente
                {
                    ItemCardapioId = itemId,
                    IngredienteId = ingredienteId
                })
                .ToList();

            if (paraAdicionar.Count > 0)
            {
                _context.ItemCardapioIngredientes.AddRange(paraAdicionar);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                ItemCardapioId = itemId,
                IngredientesVinculados = ingredienteIds.Count,
                NovosVinculos = paraAdicionar.Count
            });
        }
    }
}
