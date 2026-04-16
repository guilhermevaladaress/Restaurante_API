using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Cardapio
{
    public class CriarPratoPorPeriodoRequestDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string Descricao { get; set; } = string.Empty;

        [Range(0.01, 99999)]
        public decimal PrecoBase { get; set; }
    }
}
