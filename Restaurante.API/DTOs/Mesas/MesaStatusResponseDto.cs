namespace Restaurante.API.DTOs.Mesas
{
    public class MesaStatusResponseDto : MesaResponseDto
    {
        public bool Reservada { get; set; }
        public int? ReservaId { get; set; }
        public DateTime? DataHoraReserva { get; set; }
        public string? CodigoConfirmacaoReserva { get; set; }
    }
}
