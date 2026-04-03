using Restaurante.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Cardapio
{
    public class CriarItemCardapioRequestDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string Descricao { get; set; } = string.Empty;

        [Range(0.01, 99999)]
        public decimal PrecoBase { get; set; }

        [Required]
        public PeriodoRefeicao Periodo { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
