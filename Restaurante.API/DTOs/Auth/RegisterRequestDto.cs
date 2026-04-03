using System.ComponentModel.DataAnnotations;

namespace Restaurante.API.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
