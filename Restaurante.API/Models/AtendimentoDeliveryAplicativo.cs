namespace Restaurante.API.Models
{
    public class AtendimentoDeliveryAplicativo : Atendimento
    {
        public string NomeAplicativo { get; set; } = string.Empty;
        public string EnderecoEntrega { get; set; } = string.Empty;

        public AtendimentoDeliveryAplicativo()
        {
            Tipo = TipoAtendimento.DeliveryAplicativo;
        }

        public override decimal CalcularTaxa(decimal valorPedido)
        {
            var hora = DataHora.Hour;
            var percentual = (hora >= 18) ? 0.06m : 0.04m;
            return valorPedido * percentual;
        }
    }
}
