using Restaurante.API.Models;

namespace Restaurante.API.DTOs.SugestaoChefe
{
    public class DefinirSugestaoChefeRequestDto
    {
        public DateOnly Data { get; set; }
        public PeriodoRefeicao Periodo { get; set; }
        public int ItemCardapioId { get; set; }
    }
}
