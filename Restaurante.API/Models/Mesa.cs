namespace Restaurante.API.Models
{
    public class Mesa
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidade { get; set; }
        public bool Ativa { get; set; } = true;
    }
}
