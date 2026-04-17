using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Mesas
{
    public class CriarMesaRequestDto
    {
        [Range(1, int.MaxValue)]
        public int Numero { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacidade { get; set; }
    }
}
