namespace Restaurante.API.Models
{
    public enum StatusPedido
    {
        Pendente,
        Confirmado,
        EmPreparo,
        Entregue,
        Cancelado
    }
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;
        public decimal ValorTotal { get; set; }

        // Chave estrangeira para Usuario
        public string UsuarioId { get; set; } = string.Empty;
        public Usuario Usuario { get; set; } = null!;


        // Chave estrangeira para Atendimento
        public int AtendimentoId { get; set; }
        public Atendimento Atendimento { get; set; } = null!;


        // Relacionamento N-N com ItemCardapio
        public ICollection<PedidoItem> PedidoItens { get; set; }
            = new List<PedidoItem>();


        // Calcula o valor total considerando itens + taxa do atendimento
        public decimal CalcularValorTotal()
        {
            var subtotal = PedidoItens.Sum(pi => pi.PrecoUnitario * pi.Quantidade);
            var taxa = Atendimento?.CalcularTaxa(subtotal) ?? 0;
            return subtotal + taxa;
        }
    }
}
