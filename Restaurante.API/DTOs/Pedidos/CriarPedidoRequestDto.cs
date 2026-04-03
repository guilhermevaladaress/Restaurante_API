using Restaurante.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Pedidos
{
    public class CriarPedidoRequestDto
    {
        [Required]
        public PeriodoRefeicao Periodo { get; set; }

        [Required]
        public TipoAtendimento TipoAtendimento { get; set; }

        public int? NumeroMesa { get; set; }

        public decimal? TaxaEntrega { get; set; }

        public string? NomeAplicativo { get; set; }

        public string? EnderecoEntrega { get; set; }

        [MinLength(1)]
        public List<CriarPedidoItemDto> Itens { get; set; } = new();
    }
}
