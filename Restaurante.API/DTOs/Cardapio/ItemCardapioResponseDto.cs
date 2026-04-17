using Restaurante.API.Models;

namespace Restaurante.API.DTOs.Cardapio
{
    public class ItemCardapioResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal PrecoBase { get; set; }
        public PeriodoRefeicao Periodo { get; set; }
        public bool Ativo { get; set; }
        public IReadOnlyList<string> Ingredientes { get; set; } = Array.Empty<string>();
        public bool PossuiImagem { get; set; }
        public string? ImagemBase64 { get; set; }
        public string? ImagemMimeType { get; set; }
        public bool EhSugestaoChefeHoje { get; set; }
        public decimal PrecoComDescontoHoje { get; set; }
    }
}
