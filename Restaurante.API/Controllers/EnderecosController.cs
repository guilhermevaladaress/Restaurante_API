using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Enderecos;
using Restaurante.API.Models;
using System.Security.Claims;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnderecosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnderecosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var enderecos = await _context.EnderecosEntrega
                .AsNoTracking()
                .Where(e => e.UsuarioId == usuarioId)
                .OrderBy(e => e.Id)
                .ToListAsync();

            return Ok(enderecos);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] SalvarEnderecoRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var endereco = new EnderecoEntrega
            {
                UsuarioId = usuarioId,
                Logradouro = request.Logradouro,
                Numero = request.Numero,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                CEP = request.CEP,
                Complemento = request.Complemento
            };

            _context.EnderecosEntrega.Add(endereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Listar), new { }, endereco);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] SalvarEnderecoRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var endereco = await _context.EnderecosEntrega
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == usuarioId);

            if (endereco is null)
                return NotFound(new { message = "Endereço não encontrado." });

            endereco.Logradouro = request.Logradouro;
            endereco.Numero = request.Numero;
            endereco.Bairro = request.Bairro;
            endereco.Cidade = request.Cidade;
            endereco.CEP = request.CEP;
            endereco.Complemento = request.Complemento;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var endereco = await _context.EnderecosEntrega
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == usuarioId);

            if (endereco is null)
                return NotFound(new { message = "Endereço não encontrado." });

            _context.EnderecosEntrega.Remove(endereco);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
