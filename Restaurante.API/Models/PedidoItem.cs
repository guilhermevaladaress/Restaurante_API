namespace Restaurante.API.Models
{
    public class PedidoItem
    {
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = null!;

        public int ItemCardapioId { get; set; }
        public ItemCardapio ItemCardapio { get; set; } = null!;

        public int Quantidade { get; set; }


        // Guardamos o preço no momento do pedido
        // pois o preço do item pode mudar no futuro
        public decimal PrecoUnitario { get; set; }


        // Desconto aplicado caso seja Sugestão do Chefe
        public decimal PercentualDesconto { get; set; } = 0;

        public decimal PrecoFinal =>
            PrecoUnitario * Quantidade * (1 - PercentualDesconto / 100);
    }
}
