using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Reservas;
using Restaurante.API.Models;
using System.Security.Claims;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<ActionResult<IEnumerable<ReservaAdminResponseDto>>> Listar()
        {
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Usuario)
                .OrderBy(r => r.DataHora)
                .ThenBy(r => r.NumerMesa)
                .Select(r => new ReservaAdminResponseDto
                {
                    Id = r.Id,
                    DataHora = r.DataHora,
                    NumerMesa = r.NumerMesa,
                    NumeroPessoas = r.NumeroPessoas,
                    Status = r.Status,
                    CodigoConfirmacao = r.CodigoConfirmacao,
                    UsuarioId = r.UsuarioId,
                    NomeCliente = r.Usuario.NomeCompleto,
                    EmailCliente = r.Usuario.Email ?? string.Empty
                })
                .ToListAsync();

            return Ok(reservas);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarReservaRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            if (request.DataHora <= DateTime.Now)
                return BadRequest(new { message = "Data/Hora da reserva deve ser futura." });

            if (request.DataHora.Date <= DateTime.Now.Date)
                return BadRequest(new { message = "Reserva de almoço deve ser feita com pelo menos 1 dia de antecedência." });

            var hora = request.DataHora.TimeOfDay;
            if (hora < new TimeSpan(11, 0, 0) || hora > new TimeSpan(15, 0, 0))
                return BadRequest(new { message = "Reserva permitida apenas no almoço (11h às 15h)." });

            var inicioJanela = request.DataHora.AddHours(-2);
            var fimJanela = request.DataHora.AddHours(2);

            var mesaOcupada = await _context.Reservas.AnyAsync(r =>
                r.NumerMesa == request.NumerMesa &&
                r.Status == StatusReserva.Confirmada &&
                r.DataHora >= inicioJanela &&
                r.DataHora < fimJanela);

            if (mesaOcupada)
                return Conflict(new { message = "Mesa já reservada para este horário." });

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                DataHora = request.DataHora,
                NumerMesa = request.NumerMesa,
                NumeroPessoas = request.NumeroPessoas,
                Status = StatusReserva.Confirmada
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Minhas), new { }, MapReserva(reserva));
        }

        [HttpGet("minhas")]
        public async Task<ActionResult<IEnumerable<ReservaResponseDto>>> Minhas()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var reservas = await _context.Reservas
                .AsNoTracking()
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.DataHora)
                .Select(r => new ReservaResponseDto
                {
                    Id = r.Id,
                    DataHora = r.DataHora,
                    NumerMesa = r.NumerMesa,
                    NumeroPessoas = r.NumeroPessoas,
                    Status = r.Status,
                    CodigoConfirmacao = r.CodigoConfirmacao
                })
                .ToListAsync();

            return Ok(reservas);
        }

        [HttpPatch("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);

            if (reserva is null)
                return NotFound(new { message = "Reserva não encontrada." });

            if (reserva.Status == StatusReserva.Cancelada)
                return BadRequest(new { message = "Reserva já está cancelada." });

            reserva.Status = StatusReserva.Cancelada;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                reserva.Id,
                reserva.Status
            });
        }

        private static ReservaResponseDto MapReserva(Reserva reserva)
        {
            return new ReservaResponseDto
            {
                Id = reserva.Id,
                DataHora = reserva.DataHora,
                NumerMesa = reserva.NumerMesa,
                NumeroPessoas = reserva.NumeroPessoas,
                Status = reserva.Status,
                CodigoConfirmacao = reserva.CodigoConfirmacao
            };
        }
    }
}
