using Microsoft.AspNetCore.Identity;

namespace Restaurante.API.Models
{
    public class Usuario : IdentityUser 
    {
        public string NomeCompleto { get; set; } = string.Empty;

        public ICollection<EnderecoEntrega> Enderecos {  get; set; }
            = new List<EnderecoEntrega>();
    }
}
