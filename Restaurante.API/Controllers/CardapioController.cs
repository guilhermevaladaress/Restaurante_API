using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Cardapio;
using Restaurante.API.Models;
using Restaurante.API.Services;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardapioController : ControllerBase
    {
        private const int LimiteItensPorPeriodo = 20;
        private const long MaxImagemBytes = 5L * 1024 * 1024;
        private const long MaxUploadRequestBytes = 10L * 1024 * 1024;

        private static readonly HashSet<string> TiposImagemPermitidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };

        private readonly AppDbContext _context;
        private readonly ISugestaoChefeService _sugestaoChefeService;

        public CardapioController(
            AppDbContext context,
            ISugestaoChefeService sugestaoChefeService)
        {
            _context = context;
            _sugestaoChefeService = sugestaoChefeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCardapioResponseDto>>> Get([FromQuery] PeriodoRefeicao periodo)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var sugestaoHoje = await _sugestaoChefeService.ObterParaDataAsync(hoje, periodo);
            var sugestaoItemId = sugestaoHoje?.ItemCardapioId;
            var percentualDesconto = sugestaoHoje?.PercentualDesconto ?? 0m;

            var itens = await _context.ItensCardapio
                .AsNoTracking()
                .Include(i => i.ItemCardapioIngredientes)
                .ThenInclude(ii => ii.Ingrediente)
                .Where(i => i.Ativo && i.Periodo == periodo)
                .OrderBy(i => i.Nome)
                .ToListAsync();

            return Ok(itens.Select(item => MapItemResponse(
                item,
                sugestaoItemId,
                percentualDesconto,
                ObterNomesIngredientes(item))));
        }

        [HttpGet("{itemId:int}/midia")]
        public async Task<ActionResult<ItemCardapioMidiaResponseDto>> ObterMidia(int itemId)
        {
            var item = await _context.ItensCardapio
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item is null || (!item.Ativo && !User.IsInRole(IdentityInitializer.AdminRole)))
                return NotFound(new { message = "Item do cardapio nao encontrado." });

            return Ok(MapMidiaResponse(item));
        }

        [HttpPost("almoco")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> CriarAlmoco([FromBody] CriarPratoPorPeriodoRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            return await CriarItemAsync(
                request.Nome,
                request.Descricao,
                request.PrecoBase,
                PeriodoRefeicao.Almoco,
                ativo: true);
        }

        [HttpPost("jantar")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> CriarJantar([FromBody] CriarPratoPorPeriodoRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            return await CriarItemAsync(
                request.Nome,
                request.Descricao,
                request.PrecoBase,
                PeriodoRefeicao.Jantar,
                ativo: true);
        }

        [HttpPost]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Criar([FromBody] CriarItemCardapioRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            return await CriarItemAsync(
                request.Nome,
                request.Descricao,
                request.PrecoBase,
                request.Periodo,
                request.Ativo);
        }

        [HttpPut("{itemId:int}")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<ActionResult<ItemCardapioResponseDto>> Editar(
            int itemId,
            [FromBody] CriarItemCardapioRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var item = await _context.ItensCardapio.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item is null)
                return NotFound(new { message = "Item do cardapio nao encontrado." });

            if (request.Ativo)
            {
                var itemIgnorado = item.Ativo && item.Periodo == request.Periodo
                    ? (int?)item.Id
                    : null;

                if (await LimiteDeItensAtivosAtingidoAsync(request.Periodo, itemIgnorado))
                {
                    return Conflict(new
                    {
                        message = $"Limite de {LimiteItensPorPeriodo} itens para {request.Periodo} atingido."
                    });
                }
            }

            item.Nome = request.Nome;
            item.Descricao = request.Descricao;
            item.PrecoBase = request.PrecoBase;
            item.Periodo = request.Periodo;
            item.Ativo = request.Ativo;

            await _context.SaveChangesAsync();

            return Ok(await CriarRespostaItemAsync(item));
        }

        [HttpDelete("{itemId:int}")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Excluir(int itemId)
        {
            var item = await _context.ItensCardapio.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item is null)
                return NotFound(new { message = "Item do cardapio nao encontrado." });

            _context.ItensCardapio.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{itemId:int}/midia")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadRequestBytes)]
        [RequestSizeLimit(MaxUploadRequestBytes)]
        public async Task<ActionResult<ItemCardapioMidiaResponseDto>> AtualizarMidia(
            int itemId,
            [FromForm] AtualizarMidiaItemCardapioRequestDto request)
        {
            if (request.RemoverImagem && request.Imagem is not null)
                return BadRequest(new { message = "Escolha entre remover a imagem atual ou enviar uma nova imagem." });

            var houveSolicitacao =
                request.RemoverImagem ||
                request.Imagem is not null;

            if (!houveSolicitacao)
                return BadRequest(new { message = "Envie uma imagem ou marque a remocao da imagem atual." });

            var item = await _context.ItensCardapio.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item is null)
                return NotFound(new { message = "Item do cardapio nao encontrado." });

            try
            {
                if (request.RemoverImagem)
                {
                    item.ImagemBase64 = null;
                    item.ImagemMimeType = null;
                }

                if (request.Imagem is not null)
                {
                    var imagem = await ConverterArquivoParaBase64Async(
                        request.Imagem,
                        nomeMidia: "imagem",
                        tiposPermitidos: TiposImagemPermitidos,
                        limiteBytes: MaxImagemBytes);

                    item.ImagemBase64 = imagem.Base64;
                    item.ImagemMimeType = imagem.MimeType;
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            await _context.SaveChangesAsync();

            return Ok(MapMidiaResponse(item));
        }

        [HttpPost("{itemId:int}/ingredientes")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> VincularIngredientes(int itemId, [FromBody] VincularIngredientesRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var item = await _context.ItensCardapio.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item is null)
                return NotFound(new { message = "Item do cardapio nao encontrado." });

            var ingredienteIds = request.IngredienteIds.Distinct().ToList();
            var ingredientesExistentes = await _context.Ingredientes
                .Where(i => ingredienteIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();

            if (ingredientesExistentes.Count != ingredienteIds.Count)
                return BadRequest(new { message = "Um ou mais ingredientes informados nao existem." });

            var relacionamentosExistentes = await _context.ItemCardapioIngredientes
                .Where(ii => ii.ItemCardapioId == itemId && ingredienteIds.Contains(ii.IngredienteId))
                .Select(ii => ii.IngredienteId)
                .ToListAsync();

            var paraAdicionar = ingredienteIds.Except(relacionamentosExistentes)
                .Select(ingredienteId => new ItemCardapioIngrediente
                {
                    ItemCardapioId = itemId,
                    IngredienteId = ingredienteId
                })
                .ToList();

            if (paraAdicionar.Count > 0)
            {
                _context.ItemCardapioIngredientes.AddRange(paraAdicionar);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                ItemCardapioId = itemId,
                IngredientesVinculados = ingredienteIds.Count,
                NovosVinculos = paraAdicionar.Count
            });
        }

        private async Task<IActionResult> CriarItemAsync(
            string nome,
            string descricao,
            decimal precoBase,
            PeriodoRefeicao periodo,
            bool ativo)
        {
            if (ativo && await LimiteDeItensAtivosAtingidoAsync(periodo))
            {
                return Conflict(new
                {
                    message = $"Limite de {LimiteItensPorPeriodo} itens para {periodo} atingido."
                });
            }

            var item = new ItemCardapio
            {
                Nome = nome,
                Descricao = descricao,
                PrecoBase = precoBase,
                Periodo = periodo,
                Ativo = ativo
            };

            _context.ItensCardapio.Add(item);
            await _context.SaveChangesAsync();

            var response = await CriarRespostaItemAsync(item);
            return CreatedAtAction(nameof(Get), new { periodo = item.Periodo }, response);
        }

        private async Task<ItemCardapioResponseDto> CriarRespostaItemAsync(ItemCardapio item)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var sugestaoHoje = await _sugestaoChefeService.ObterParaDataAsync(hoje, item.Periodo);

            var nomesIngredientes = await _context.ItemCardapioIngredientes
                .AsNoTracking()
                .Where(ii => ii.ItemCardapioId == item.Id)
                .Select(ii => ii.Ingrediente.Nome)
                .Distinct()
                .OrderBy(nome => nome)
                .ToListAsync();

            return MapItemResponse(
                item,
                sugestaoHoje?.ItemCardapioId,
                sugestaoHoje?.PercentualDesconto ?? 0m,
                nomesIngredientes);
        }

        private static ItemCardapioResponseDto MapItemResponse(
            ItemCardapio item,
            int? sugestaoItemId,
            decimal percentualDesconto,
            IReadOnlyList<string> ingredientes)
        {
            var ehSugestaoHoje = sugestaoItemId.HasValue && sugestaoItemId.Value == item.Id;

            return new ItemCardapioResponseDto
            {
                Id = item.Id,
                Nome = item.Nome,
                Descricao = item.Descricao,
                PrecoBase = item.PrecoBase,
                Periodo = item.Periodo,
                Ativo = item.Ativo,
                Ingredientes = ingredientes,
                PossuiImagem = !string.IsNullOrWhiteSpace(item.ImagemBase64),
                ImagemBase64 = item.ImagemBase64,
                ImagemMimeType = item.ImagemMimeType,
                EhSugestaoChefeHoje = ehSugestaoHoje,
                PrecoComDescontoHoje = ehSugestaoHoje
                    ? item.PrecoBase * (1 - percentualDesconto / 100m)
                    : item.PrecoBase
            };
        }

        private static IReadOnlyList<string> ObterNomesIngredientes(ItemCardapio item)
        {
            return item.ItemCardapioIngredientes
                .Select(ii => ii.Ingrediente.Nome)
                .Where(nome => !string.IsNullOrWhiteSpace(nome))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(nome => nome)
                .ToList();
        }

        private async Task<bool> LimiteDeItensAtivosAtingidoAsync(PeriodoRefeicao periodo, int? itemIdIgnorado = null)
        {
            var quantidade = await _context.ItensCardapio
                .AsNoTracking()
                .Where(i => i.Ativo && i.Periodo == periodo)
                .Where(i => !itemIdIgnorado.HasValue || i.Id != itemIdIgnorado.Value)
                .CountAsync();

            return quantidade >= LimiteItensPorPeriodo;
        }

        private static ItemCardapioMidiaResponseDto MapMidiaResponse(ItemCardapio item)
        {
            return new ItemCardapioMidiaResponseDto
            {
                Id = item.Id,
                Nome = item.Nome,
                PossuiImagem = !string.IsNullOrWhiteSpace(item.ImagemBase64),
                ImagemBase64 = item.ImagemBase64,
                ImagemMimeType = item.ImagemMimeType
            };
        }

        private static async Task<(string Base64, string MimeType)> ConverterArquivoParaBase64Async(
            IFormFile arquivo,
            string nomeMidia,
            ISet<string> tiposPermitidos,
            long limiteBytes)
        {
            if (arquivo.Length <= 0)
                throw new InvalidOperationException($"O arquivo de {nomeMidia} esta vazio.");

            if (arquivo.Length > limiteBytes)
                throw new InvalidOperationException($"O arquivo de {nomeMidia} excede o limite permitido.");

            if (string.IsNullOrWhiteSpace(arquivo.ContentType) || !tiposPermitidos.Contains(arquivo.ContentType))
                throw new InvalidOperationException($"O tipo de arquivo enviado para {nomeMidia} nao e suportado.");

            using var memoryStream = new MemoryStream();
            await arquivo.CopyToAsync(memoryStream);

            return (Convert.ToBase64String(memoryStream.ToArray()), arquivo.ContentType);
        }
    }
}
