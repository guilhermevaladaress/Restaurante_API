namespace Restaurante.API.Models
{

    public enum TipoAtendimento
    {
        Presencial,
        DeliveryProprio,
        DeliveryAplicativo
    }
    public abstract class Atendimento
    {
        public int Id { get; set; }
        public TipoAtendimento Tipo { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;

        // Calcula a taxa — cada subclasse implementa do seu jeito
        public abstract decimal CalcularTaxa(decimal valorPedido);

        // Relacionamento com Pedido
        public Pedido Pedido { get; set; } = null!;
    }
}
