using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Enderecos
{
    public class SalvarEnderecoRequestDto
    {
        [Required]
        public string Logradouro { get; set; } = string.Empty;

        [Required]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public string Bairro { get; set; } = string.Empty;

        [Required]
        public string Cidade { get; set; } = string.Empty;

        [Required]
        public string CEP { get; set; } = string.Empty;

        public string? Complemento { get; set; }
    }
}
