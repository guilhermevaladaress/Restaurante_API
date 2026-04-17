using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Mesas;
using Restaurante.API.Models;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MesasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MesasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<ActionResult<MesaResponseDto>> Criar([FromBody] CriarMesaRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var numeroJaExiste = await _context.Mesas
                .AsNoTracking()
                .AnyAsync(m => m.Numero == request.Numero);

            if (numeroJaExiste)
                return Conflict(new { message = "Já existe uma mesa cadastrada com esse número." });

            var mesa = new Mesa
            {
                Numero = request.Numero,
                Capacidade = request.Capacidade,
                Ativa = true
            };

            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorNumero), new { numero = mesa.Numero }, MapMesa(mesa));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MesaStatusResponseDto>>> Listar([FromQuery] DateTime? dataHora = null)
        {
            var mesas = await _context.Mesas
                .AsNoTracking()
                .OrderBy(m => m.Numero)
                .Select(m => new MesaResponseDto
                {
                    Id = m.Id,
                    Numero = m.Numero,
                    Capacidade = m.Capacidade,
                    Ativa = m.Ativa
                })
                .ToListAsync();

            if (!dataHora.HasValue)
            {
                return Ok(mesas.Select(m => new MesaStatusResponseDto
                {
                    Id = m.Id,
                    Numero = m.Numero,
                    Capacidade = m.Capacidade,
                    Ativa = m.Ativa,
                    Reservada = false
                }));
            }

            var inicioDia = dataHora.Value.Date;
            var fimDia = inicioDia.AddDays(1);

            var reservas = await _context.Reservas
                .AsNoTracking()
                .Where(r => r.Status == StatusReserva.Confirmada
                            && r.DataHora >= inicioDia
                            && r.DataHora < fimDia)
                .GroupBy(r => r.NumerMesa)
                .Select(g => g.OrderBy(r => r.DataHora).First())
                .ToListAsync();

            var reservasPorMesa = reservas.ToDictionary(r => r.NumerMesa, r => r);

            var resultado = mesas.Select(m =>
            {
                reservasPorMesa.TryGetValue(m.Numero, out var reserva);

                return new MesaStatusResponseDto
                {
                    Id = m.Id,
                    Numero = m.Numero,
                    Capacidade = m.Capacidade,
                    Ativa = m.Ativa,
                    Reservada = reserva is not null,
                    ReservaId = reserva?.Id,
                    DataHoraReserva = reserva?.DataHora,
                    CodigoConfirmacaoReserva = reserva?.CodigoConfirmacao
                };
            });

            return Ok(resultado);
        }

        [HttpGet("{numero:int}")]
        public async Task<ActionResult<MesaResponseDto>> ObterPorNumero(int numero)
        {
            var mesa = await _context.Mesas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Numero == numero);

            if (mesa is null)
                return NotFound(new { message = "Mesa não encontrada." });

            return Ok(MapMesa(mesa));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<ActionResult<MesaResponseDto>> Atualizar(int id, [FromBody] AtualizarMesaRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var mesa = await _context.Mesas.FirstOrDefaultAsync(m => m.Id == id);
            if (mesa is null)
                return NotFound(new { message = "Mesa não encontrada." });

            var numeroEmUso = await _context.Mesas
                .AsNoTracking()
                .AnyAsync(m => m.Numero == request.Numero && m.Id != id);

            if (numeroEmUso)
                return Conflict(new { message = "Já existe uma mesa cadastrada com esse número." });

            mesa.Numero = request.Numero;
            mesa.Capacidade = request.Capacidade;
            mesa.Ativa = request.Ativa;

            await _context.SaveChangesAsync();

            return Ok(MapMesa(mesa));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<IActionResult> Excluir(int id)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(m => m.Id == id);
            if (mesa is null)
                return NotFound(new { message = "Mesa não encontrada." });

            _context.Mesas.Remove(mesa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static MesaResponseDto MapMesa(Mesa mesa)
        {
            return new MesaResponseDto
            {
                Id = mesa.Id,
                Numero = mesa.Numero,
                Capacidade = mesa.Capacidade,
                Ativa = mesa.Ativa
            };
        }
    }
}
