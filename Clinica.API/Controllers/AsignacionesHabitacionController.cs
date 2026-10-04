using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsignacionesHabitacionController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public AsignacionesHabitacionController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "AsignacionesHabitacionConsultar")]
        public async Task<IActionResult> GetAsignaciones()
        {
            var asignaciones = await _context.AsignacionesHabitacion
                .Include(a => a.Paciente)
                .Include(a => a.Habitacion)
                .OrderByDescending(a => a.FechaIngreso)
                .Select(a => new
                {
                    a.IdAsignacion,
                    a.IdPaciente,
                    Paciente = a.Paciente != null
                        ? a.Paciente.Nombres + " " + a.Paciente.Apellidos
                        : null,
                    a.IdHabitacion,
                    Habitacion = a.Habitacion != null
                        ? a.Habitacion.Numero
                        : null,
                    a.FechaIngreso,
                    a.FechaEgreso,
                    a.Estado
                })
                .ToListAsync();

            return Ok(asignaciones);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AsignacionesHabitacionConsultar")]
        public async Task<IActionResult> GetAsignacion(int id)
        {
            var asignacion = await _context.AsignacionesHabitacion
                .Include(a => a.Paciente)
                .Include(a => a.Habitacion)
                .Where(a => a.IdAsignacion == id)
                .Select(a => new
                {
                    a.IdAsignacion,
                    a.IdPaciente,
                    Paciente = a.Paciente != null
                        ? a.Paciente.Nombres + " " + a.Paciente.Apellidos
                        : null,
                    a.IdHabitacion,
                    Habitacion = a.Habitacion != null
                        ? a.Habitacion.Numero
                        : null,
                    a.FechaIngreso,
                    a.FechaEgreso,
                    a.Estado
                })
                .FirstOrDefaultAsync();

            if (asignacion == null)
            {
                return NotFound();
            }

            return Ok(asignacion);
        }

        [HttpPost]
        [Authorize(Policy = "AsignacionesHabitacionCrear")]
        public async Task<IActionResult> CrearAsignacion(
            AsignacionHabitacion asignacion)
        {
            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == asignacion.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            var habitacion = await _context.Habitaciones
                .FindAsync(asignacion.IdHabitacion);

            if (habitacion == null)
            {
                return BadRequest(new
                {
                    mensaje = "La habitación indicada no existe."
                });
            }

            if (habitacion.Estado != "Libre")
            {
                return BadRequest(new
                {
                    mensaje = "La habitación indicada no está libre."
                });
            }

            if (asignacion.FechaIngreso == default)
            {
                asignacion.FechaIngreso = DateTime.Now;
            }

            asignacion.Estado = "Activa";
            habitacion.Estado = "Ocupada";

            _context.AsignacionesHabitacion.Add(asignacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAsignacion),
                new { id = asignacion.IdAsignacion },
                new
                {
                    asignacion.IdAsignacion,
                    asignacion.IdPaciente,
                    asignacion.IdHabitacion,
                    asignacion.FechaIngreso,
                    asignacion.FechaEgreso,
                    asignacion.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AsignacionesHabitacionModificar")]
        public async Task<IActionResult> ModificarAsignacion(
            int id,
            AsignacionHabitacion asignacion)
        {
            if (id != asignacion.IdAsignacion)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la asignación."
                });
            }

            var asignacionExistente = await _context.AsignacionesHabitacion
                .FindAsync(id);

            if (asignacionExistente == null)
            {
                return NotFound();
            }

            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == asignacion.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            var habitacion = await _context.Habitaciones
                .FindAsync(asignacion.IdHabitacion);

            if (habitacion == null)
            {
                return BadRequest(new
                {
                    mensaje = "La habitación indicada no existe."
                });
            }

            asignacionExistente.IdPaciente = asignacion.IdPaciente;
            asignacionExistente.IdHabitacion = asignacion.IdHabitacion;
            asignacionExistente.FechaIngreso = asignacion.FechaIngreso;
            asignacionExistente.FechaEgreso = asignacion.FechaEgreso;
            asignacionExistente.Estado = asignacion.Estado;

            if (asignacion.Estado == "Finalizada")
            {
                habitacion.Estado = "Libre";
            }
            else if (asignacion.Estado == "Activa")
            {
                habitacion.Estado = "Ocupada";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                asignacionExistente.IdAsignacion,
                asignacionExistente.IdPaciente,
                asignacionExistente.IdHabitacion,
                asignacionExistente.FechaIngreso,
                asignacionExistente.FechaEgreso,
                asignacionExistente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AsignacionesHabitacionEliminar")]
        public async Task<IActionResult> EliminarAsignacion(int id)
        {
            var asignacion = await _context.AsignacionesHabitacion
                .FindAsync(id);

            if (asignacion == null)
            {
                return NotFound();
            }

            var habitacion = await _context.Habitaciones
                .FindAsync(asignacion.IdHabitacion);

            if (habitacion != null)
            {
                habitacion.Estado = "Libre";
            }

            _context.AsignacionesHabitacion.Remove(asignacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}