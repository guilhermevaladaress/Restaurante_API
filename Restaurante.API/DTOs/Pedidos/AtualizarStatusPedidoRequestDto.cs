using Restaurante.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Pedidos
{
    public class AtualizarStatusPedidoRequestDto
    {
        [Required]
        public StatusPedido Status { get; set; }
    }
}
