namespace Restaurante.API.Models
{
    public class Ingrediente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        public ICollection<ItemCardapioIngrediente> ItemCardapioIngredientes { get; set; }
            = new List<ItemCardapioIngrediente>();
    }
}
