using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Ingredientes
{
    public class SalvarIngredienteRequestDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;
    }
}
