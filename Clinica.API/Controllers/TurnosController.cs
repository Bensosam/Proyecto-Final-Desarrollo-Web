using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public TurnosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "TurnosConsultar")]
        public async Task<IActionResult> GetTurnos()
        {
            var turnos = await _context.Turnos
                .OrderBy(t => t.IdTurno)
                .Select(t => new
                {
                    t.IdTurno,
                    t.Nombre,
                    t.HoraInicio,
                    t.HoraFin,
                    t.Estado,
                    CantidadEmpleados = t.Empleados.Count
                })
                .ToListAsync();

            return Ok(turnos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "TurnosConsultar")]
        public async Task<IActionResult> GetTurno(int id)
        {
            var turno = await _context.Turnos
                .Where(t => t.IdTurno == id)
                .Select(t => new
                {
                    t.IdTurno,
                    t.Nombre,
                    t.HoraInicio,
                    t.HoraFin,
                    t.Estado,
                    CantidadEmpleados = t.Empleados.Count
                })
                .FirstOrDefaultAsync();

            if (turno == null)
            {
                return NotFound();
            }

            return Ok(turno);
        }

        [HttpPost]
        [Authorize(Policy = "TurnosCrear")]
        public async Task<IActionResult> CrearTurno(
            Turno turno)
        {
            if (string.IsNullOrWhiteSpace(turno.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del turno es obligatorio."
                });
            }

            if (turno.HoraInicio == turno.HoraFin)
            {
                return BadRequest(new
                {
                    mensaje = "La hora de inicio y la hora de fin no pueden ser iguales."
                });
            }

            var nombre = turno.Nombre.Trim();

            var nombreExiste = await _context.Turnos
                .AnyAsync(t => t.Nombre == nombre);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe un turno con ese nombre."
                });
            }

            turno.Nombre = nombre;

            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTurno),
                new { id = turno.IdTurno },
                new
                {
                    turno.IdTurno,
                    turno.Nombre,
                    turno.HoraInicio,
                    turno.HoraFin,
                    turno.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "TurnosModificar")]
        public async Task<IActionResult> ModificarTurno(
            int id,
            Turno turno)
        {
            if (id != turno.IdTurno)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del turno."
                });
            }

            var existente = await _context.Turnos
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(turno.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del turno es obligatorio."
                });
            }

            if (turno.HoraInicio == turno.HoraFin)
            {
                return BadRequest(new
                {
                    mensaje = "La hora de inicio y la hora de fin no pueden ser iguales."
                });
            }

            var nombre = turno.Nombre.Trim();

            var nombreExiste = await _context.Turnos
                .AnyAsync(t =>
                    t.Nombre == nombre &&
                    t.IdTurno != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otro turno con ese nombre."
                });
            }

            existente.Nombre = nombre;
            existente.HoraInicio = turno.HoraInicio;
            existente.HoraFin = turno.HoraFin;
            existente.Estado = turno.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdTurno,
                existente.Nombre,
                existente.HoraInicio,
                existente.HoraFin,
                existente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "TurnosEliminar")]
        public async Task<IActionResult> EliminarTurno(int id)
        {
            var turno = await _context.Turnos
                .FindAsync(id);

            if (turno == null)
            {
                return NotFound();
            }

            var tieneEmpleados = await _context.Empleados
                .AnyAsync(e => e.IdTurno == id);

            if (tieneEmpleados)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar el turno porque tiene empleados asociados."
                });
            }

            _context.Turnos.Remove(turno);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}