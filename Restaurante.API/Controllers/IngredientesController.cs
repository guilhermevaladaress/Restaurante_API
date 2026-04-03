using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Ingredientes;
using Restaurante.API.Models;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngredientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var ingredientes = await _context.Ingredientes
                .AsNoTracking()
                .OrderBy(i => i.Nome)
                .Select(i => new { i.Id, i.Nome })
                .ToListAsync();

            return Ok(ingredientes);
        }

        [HttpPost]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Criar([FromBody] SalvarIngredienteRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var nomeNormalizado = request.Nome.Trim();
            var jaExiste = await _context.Ingredientes
                .AnyAsync(i => i.Nome == nomeNormalizado);

            if (jaExiste)
                return Conflict(new { message = "Ingrediente já cadastrado." });

            var ingrediente = new Ingrediente { Nome = nomeNormalizado };

            _context.Ingredientes.Add(ingrediente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Listar), new { }, new { ingrediente.Id, ingrediente.Nome });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] SalvarIngredienteRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var ingrediente = await _context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id);
            if (ingrediente is null)
                return NotFound(new { message = "Ingrediente não encontrado." });

            var nomeNormalizado = request.Nome.Trim();
            var nomeDuplicado = await _context.Ingredientes
                .AnyAsync(i => i.Id != id && i.Nome == nomeNormalizado);

            if (nomeDuplicado)
                return Conflict(new { message = "Já existe outro ingrediente com esse nome." });

            ingrediente.Nome = nomeNormalizado;
            await _context.SaveChangesAsync();

            return Ok(new { ingrediente.Id, ingrediente.Nome });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Excluir(int id)
        {
            var ingrediente = await _context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id);
            if (ingrediente is null)
                return NotFound(new { message = "Ingrediente não encontrado." });

            var emUso = await _context.ItemCardapioIngredientes.AnyAsync(ii => ii.IngredienteId == id);
            if (emUso)
                return BadRequest(new { message = "Ingrediente não pode ser removido pois está vinculado a itens do cardápio." });

            _context.Ingredientes.Remove(ingrediente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
