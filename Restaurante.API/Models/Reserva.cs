namespace Restaurante.API.Models
{
    public enum StatusReserva
    {
        Confirmada,
        Cancelada
    }

    public class Reserva
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public int NumerMesa { get; set; }
        public int NumeroPessoas { get; set; }
        public StatusReserva Status { get; set; } = StatusReserva.Confirmada;

        // Código gerado automaticamente para confirmação
        public string CodigoConfirmacao { get; set; }
            = Guid.NewGuid().ToString("N")[..8].ToUpper();

        // Chave estrangeira para Usuario
        public string UsuarioId { get; set; } = string.Empty;
        public Usuario Usuario { get; set; } = null!;
    }
}
