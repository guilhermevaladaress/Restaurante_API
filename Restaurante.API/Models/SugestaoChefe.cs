namespace Restaurante.API.Models
{
    public class SugestaoChefe
    {
        public int Id { get; set; }
        public DateOnly Data { get; set; }
        public PeriodoRefeicao Periodo { get; set; }
        public decimal PercentualDesconto { get; set; } = 20;


        // Qual item foi escolhido como sugestão
        public int ItemCardapioId { get; set; }
        public ItemCardapio ItemCardapio { get; set; } = null!;
    }
}
