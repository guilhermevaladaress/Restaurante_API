namespace Restaurante.API.Models
{
    public class AtendimentoPresencial : Atendimento
    {
        public int NumerMesa { get; set; }

        public AtendimentoPresencial ()
        {
            Tipo = TipoAtendimento.Presencial;
        }

        public override decimal CalcularTaxa(decimal valorPedido) => 0;
    }
}
