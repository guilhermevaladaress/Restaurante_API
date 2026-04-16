using Microsoft.AspNetCore.Http;

namespace Restaurante.API.DTOs.Cardapio
{
    public class AtualizarMidiaItemCardapioRequestDto
    {
        public IFormFile? Imagem { get; set; }
        public bool RemoverImagem { get; set; }
    }
}
