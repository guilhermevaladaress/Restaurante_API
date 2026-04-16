namespace Restaurante.API.Models
{
    
    public enum PeriodoRefeicao
    {
        Almoco,
        Jantar
    }
    
    public class ItemCardapio
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal PrecoBase { get; set; }
        public PeriodoRefeicao Periodo { get; set; }
        public bool Ativo { get; set; } = true;
        public string? ImagemBase64 { get; set; }
        public string? ImagemMimeType { get; set; }


        public ICollection<ItemCardapioIngrediente> ItemCardapioIngredientes { get; set; }
            = new List<ItemCardapioIngrediente>();

        public ICollection<PedidoItem> PedidoItens { get; set; }
            = new List<PedidoItem>();
    }
}
