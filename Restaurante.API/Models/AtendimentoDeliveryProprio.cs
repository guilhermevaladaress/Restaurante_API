namespace Restaurante.API.Models
{
    public class AtendimentoDeliveryProprio : Atendimento
    {
    public const decimal TaxaFixaEntrega = 8m;
        public decimal TaxaEntrega { get; set; }
        public string EnderecoEntrega { get; set; } = string.Empty;

        public AtendimentoDeliveryProprio()
        {
            Tipo = TipoAtendimento.DeliveryProprio;
        TaxaEntrega = TaxaFixaEntrega;
        }

    public override decimal CalcularTaxa(decimal valorPedido) => TaxaFixaEntrega;
    }
}
