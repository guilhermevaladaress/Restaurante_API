using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Pedidos
{
    public class CriarPedidoItemDto
    {
        [Range(1, int.MaxValue)]
        public int ItemCardapioId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }
    }
}
