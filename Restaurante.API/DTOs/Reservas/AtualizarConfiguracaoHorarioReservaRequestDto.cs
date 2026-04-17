using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Reservas
{
    public class AtualizarConfiguracaoHorarioReservaRequestDto
    {
        [Required]
        public string HoraInicio { get; set; } = string.Empty;

        [Required]
        public string HoraFim { get; set; } = string.Empty;
    }
}
