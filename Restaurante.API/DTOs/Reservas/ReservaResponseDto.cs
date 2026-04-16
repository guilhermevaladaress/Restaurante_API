using Restaurante.API.Models;

namespace Restaurante.API.DTOs.Reservas
{
    public class ReservaResponseDto
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public int NumerMesa { get; set; }
        public int NumeroPessoas { get; set; }
        public StatusReserva Status { get; set; }
        public string CodigoConfirmacao { get; set; } = string.Empty;
    }
}
