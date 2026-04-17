using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Data;
using Restaurante.API.DTOs.Reservas;
using Restaurante.API.Models;
using System.Globalization;
using System.Security.Claims;

namespace Restaurante.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private static readonly TimeSpan HorarioReservaPadraoInicio = new(11, 0, 0);
        private static readonly TimeSpan HorarioReservaPadraoFim = new(15, 0, 0);

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

            var mesa = await _context.Mesas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Numero == request.NumerMesa && m.Ativa);

            if (mesa is null)
                return BadRequest(new { message = "Mesa não cadastrada ou inativa." });

            if (request.NumeroPessoas > mesa.Capacidade)
            {
                return BadRequest(new
                {
                    message = $"Quantidade de pessoas ({request.NumeroPessoas}) excede a capacidade da mesa ({mesa.Capacidade})."
                });
            }

            var inicioDia = request.DataHora.Date;
            var fimDia = inicioDia.AddDays(1);

            var reservaExistente = await _context.Reservas
                .AsNoTracking()
                .Where(r =>
                r.NumerMesa == request.NumerMesa &&
                r.Status == StatusReserva.Confirmada &&
                r.DataHora >= inicioDia &&
                r.DataHora < fimDia)
                .OrderBy(r => r.DataHora)
                .Select(r => new
                {
                    r.Id,
                    r.DataHora,
                    r.CodigoConfirmacao
                })
                .FirstOrDefaultAsync();

            if (reservaExistente is not null)
                return Conflict(new
                {
                    message = "Mesa já reservada para este dia.",
                    reserva = reservaExistente
                });

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

        [HttpGet("configuracao-horario")]
        [AllowAnonymous]
        public async Task<ActionResult<ConfiguracaoHorarioReservaResponseDto>> ObterConfiguracaoHorario()
        {
            var configuracao = await ObterConfiguracaoHorarioReservaAsync();

            return Ok(new ConfiguracaoHorarioReservaResponseDto
            {
                HoraInicio = FormatarHorario(configuracao.HoraInicio),
                HoraFim = FormatarHorario(configuracao.HoraFim)
            });
        }

        [HttpPut("configuracao-horario")]
        [Authorize(Roles = IdentityInitializer.AdminRole)]
        public async Task<ActionResult<ConfiguracaoHorarioReservaResponseDto>> AtualizarConfiguracaoHorario(
            [FromBody] AtualizarConfiguracaoHorarioReservaRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (!TryParseHorario(request.HoraInicio, out var horaInicio) ||
                !TryParseHorario(request.HoraFim, out var horaFim))
            {
                return BadRequest(new { message = "Informe os horários no formato HH:mm." });
            }

            if (horaInicio >= horaFim)
                return BadRequest(new { message = "Hora de inicio deve ser menor que hora de fim." });

            var configuracao = await _context.ConfiguracoesHorarioReserva.FirstOrDefaultAsync();
            if (configuracao is null)
            {
                configuracao = new ConfiguracaoHorarioReserva();
                _context.ConfiguracoesHorarioReserva.Add(configuracao);
            }

            configuracao.HoraInicio = horaInicio;
            configuracao.HoraFim = horaFim;

            await _context.SaveChangesAsync();

            return Ok(new ConfiguracaoHorarioReservaResponseDto
            {
                HoraInicio = FormatarHorario(configuracao.HoraInicio),
                HoraFim = FormatarHorario(configuracao.HoraFim)
            });
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

        private async Task<ConfiguracaoHorarioReserva> ObterConfiguracaoHorarioReservaAsync()
        {
            var configuracao = await _context.ConfiguracoesHorarioReserva
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return configuracao ?? new ConfiguracaoHorarioReserva
            {
                HoraInicio = HorarioReservaPadraoInicio,
                HoraFim = HorarioReservaPadraoFim
            };
        }

        private static string FormatarHorario(TimeSpan horario)
        {
            return $"{horario:hh\\:mm}";
        }

        private static bool TryParseHorario(string valor, out TimeSpan horario)
        {
            return TimeSpan.TryParseExact(
                valor,
                "hh\\:mm",
                CultureInfo.InvariantCulture,
                out horario);
        }
    }
}
