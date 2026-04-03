using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Cardapio
{
    public class VincularIngredientesRequestDto
    {
        [MinLength(1)]
        public List<int> IngredienteIds { get; set; } = new();
    }
}
