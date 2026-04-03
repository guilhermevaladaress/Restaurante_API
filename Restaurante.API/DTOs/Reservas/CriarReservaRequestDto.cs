using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Reservas
{
    public class CriarReservaRequestDto
    {
        [Required]
        public DateTime DataHora { get; set; }

        [Range(1, int.MaxValue)]
        public int NumerMesa { get; set; }

        [Range(1, int.MaxValue)]
        public int NumeroPessoas { get; set; }
    }
}
