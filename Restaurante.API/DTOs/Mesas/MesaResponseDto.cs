namespace Restaurante.API.DTOs.Mesas
{
    public class MesaResponseDto
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidade { get; set; }
        public bool Ativa { get; set; }
    }
}
