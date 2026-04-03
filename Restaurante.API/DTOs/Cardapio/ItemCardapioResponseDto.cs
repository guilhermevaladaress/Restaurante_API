using Restaurante.API.Models;

namespace Restaurante.API.DTOs.Cardapio
{
    public class ItemCardapioResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal PrecoBase { get; set; }
        public PeriodoRefeicao Periodo { get; set; }
        public bool EhSugestaoChefeHoje { get; set; }
        public decimal PrecoComDescontoHoje { get; set; }
    }
}
