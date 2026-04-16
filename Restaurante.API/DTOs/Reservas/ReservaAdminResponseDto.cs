namespace Restaurante.API.DTOs.Reservas
{
    public class ReservaAdminResponseDto : ReservaResponseDto
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string NomeCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
    }
}
