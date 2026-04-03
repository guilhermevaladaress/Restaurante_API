using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.Models;

namespace Restaurante.API.Services
{
    public interface ISugestaoChefeService
    {
        Task<SugestaoChefeSelecionada?> ObterParaDataAsync(
            DateOnly data,
            PeriodoRefeicao periodo,
            CancellationToken cancellationToken = default);
    }

    public sealed class SugestaoChefeService : ISugestaoChefeService
    {
        private const decimal PercentualDescontoAutomatico = 20m;
        private readonly AppDbContext _context;

        public SugestaoChefeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SugestaoChefeSelecionada?> ObterParaDataAsync(
            DateOnly data,
            PeriodoRefeicao periodo,
            CancellationToken cancellationToken = default)
        {
            var sugestaoManual = await _context.SugestoesChefe
                .AsNoTracking()
                .Include(s => s.ItemCardapio)
                .FirstOrDefaultAsync(
                    s => s.Data == data && s.Periodo == periodo,
                    cancellationToken);

            if (sugestaoManual is not null)
            {
                return new SugestaoChefeSelecionada
                {
                    Id = sugestaoManual.Id,
                    Data = sugestaoManual.Data,
                    Periodo = sugestaoManual.Periodo,
                    PercentualDesconto = sugestaoManual.PercentualDesconto,
                    ItemCardapioId = sugestaoManual.ItemCardapioId,
                    Nome = sugestaoManual.ItemCardapio.Nome,
                    Descricao = sugestaoManual.ItemCardapio.Descricao,
                    PrecoBase = sugestaoManual.ItemCardapio.PrecoBase,
                    Automatica = false
                };
            }

            var itensDisponiveis = await _context.ItensCardapio
                .AsNoTracking()
                .Where(i => i.Ativo && i.Periodo == periodo)
                .OrderBy(i => i.Id)
                .Select(i => new ItemAutomatico
                {
                    Id = i.Id,
                    Nome = i.Nome,
                    Descricao = i.Descricao,
                    PrecoBase = i.PrecoBase
                })
                .ToListAsync(cancellationToken);

            if (itensDisponiveis.Count == 0)
                return null;

            var indice = data.DayNumber % itensDisponiveis.Count;
            var itemSelecionado = itensDisponiveis[indice];

            return new SugestaoChefeSelecionada
            {
                Data = data,
                Periodo = periodo,
                PercentualDesconto = PercentualDescontoAutomatico,
                ItemCardapioId = itemSelecionado.Id,
                Nome = itemSelecionado.Nome,
                Descricao = itemSelecionado.Descricao,
                PrecoBase = itemSelecionado.PrecoBase,
                Automatica = true
            };
        }

        private sealed class ItemAutomatico
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Descricao { get; set; } = string.Empty;
            public decimal PrecoBase { get; set; }
        }
    }

    public sealed class SugestaoChefeSelecionada
    {
        public int? Id { get; init; }
        public DateOnly Data { get; init; }
        public PeriodoRefeicao Periodo { get; init; }
        public decimal PercentualDesconto { get; init; }
        public int ItemCardapioId { get; init; }
        public string Nome { get; init; } = string.Empty;
        public string Descricao { get; init; } = string.Empty;
        public decimal PrecoBase { get; init; }
        public bool Automatica { get; init; }
    }
}
