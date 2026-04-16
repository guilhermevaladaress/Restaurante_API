namespace Restaurante.API.DTOs.Cardapio
{
    public class ItemCardapioMidiaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool PossuiImagem { get; set; }
        public string? ImagemBase64 { get; set; }
        public string? ImagemMimeType { get; set; }
    }
}
