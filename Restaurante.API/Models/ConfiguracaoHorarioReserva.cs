namespace Restaurante.API.Models
{
    public class ConfiguracaoHorarioReserva
    {
        public int Id { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
    }
}
