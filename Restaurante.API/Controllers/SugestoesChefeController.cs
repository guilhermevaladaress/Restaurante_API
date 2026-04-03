using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.SugestaoChefe;
using Restaurante.API.Models;
using Restaurante.API.Services;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/sugestoes-chefe")]
    public class SugestoesChefeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISugestaoChefeService _sugestaoChefeService;

        public SugestoesChefeController(
            AppDbContext context,
            ISugestaoChefeService sugestaoChefeService)
        {
            _context = context;
            _sugestaoChefeService = sugestaoChefeService;
        }

        [HttpGet("hoje")]
        public async Task<IActionResult> ObterHoje([FromQuery] PeriodoRefeicao periodo)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var sugestao = await _sugestaoChefeService.ObterParaDataAsync(hoje, periodo);

            if (sugestao is null)
                return NotFound(new { message = "Não há Sugestão do Chefe para hoje nesse período." });

            return Ok(new
            {
                Id = sugestao.Id ?? 0,
                sugestao.Data,
                sugestao.Periodo,
                sugestao.PercentualDesconto,
                sugestao.Automatica,
                Item = new
                {
                    Id = sugestao.ItemCardapioId,
                    sugestao.Nome,
                    sugestao.Descricao,
                    sugestao.PrecoBase,
                    PrecoComDesconto = sugestao.PrecoBase * (1 - sugestao.PercentualDesconto / 100)
                }
            });
        }

        [HttpPost]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Definir([FromBody] DefinirSugestaoChefeRequestDto request)
        {
            if (request.ItemCardapioId <= 0)
                return BadRequest(new { message = "ItemCardapioId inválido." });

            var item = await _context.ItensCardapio
                .FirstOrDefaultAsync(i => i.Id == request.ItemCardapioId && i.Ativo);

            if (item is null)
                return NotFound(new { message = "Item do cardápio não encontrado ou inativo." });

            if (item.Periodo != request.Periodo)
                return BadRequest(new { message = "O item informado não pertence ao período selecionado." });

            var jaExiste = await _context.SugestoesChefe
                .AnyAsync(s => s.Data == request.Data && s.Periodo == request.Periodo);

            if (jaExiste)
                return Conflict(new { message = "Já existe Sugestão do Chefe para essa data e período." });

            var sugestao = new SugestaoChefe
            {
                Data = request.Data,
                Periodo = request.Periodo,
                ItemCardapioId = request.ItemCardapioId,
                PercentualDesconto = 20m
            };

            _context.SugestoesChefe.Add(sugestao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObterHoje),
                new { periodo = request.Periodo },
                new
                {
                    sugestao.Id,
                    sugestao.Data,
                    sugestao.Periodo,
                    sugestao.ItemCardapioId,
                    sugestao.PercentualDesconto
                });
        }
    }
}
