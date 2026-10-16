using Clinica.API.Data;
using Clinica.API.Hubs;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsignacionesHabitacionController : ControllerBase
    {
        private readonly ClinicaDbContext _context;
        private readonly IHubContext<HabitacionesHub> _hubContext;

        public AsignacionesHabitacionController(
            ClinicaDbContext context,
            IHubContext<HabitacionesHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Authorize(Policy = "AsignacionesHabitacionConsultar")]
        public async Task<IActionResult> GetAsignaciones()
        {
            var asignaciones = await _context.AsignacionesHabitacion
                .Include(a => a.Paciente)
                .Include(a => a.Habitacion)
                    .ThenInclude(h => h!.TipoHabitacion)
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
                    TipoHabitacion =
                        a.Habitacion != null &&
                        a.Habitacion.TipoHabitacion != null
                            ? a.Habitacion.TipoHabitacion.Nombre
                            : null,
                    PrecioPorDia =
                        a.Habitacion != null &&
                        a.Habitacion.TipoHabitacion != null
                            ? a.Habitacion.TipoHabitacion.PrecioPorDia
                            : 0,
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
                    .ThenInclude(h => h!.TipoHabitacion)
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
                    TipoHabitacion =
                        a.Habitacion != null &&
                        a.Habitacion.TipoHabitacion != null
                            ? a.Habitacion.TipoHabitacion.Nombre
                            : null,
                    PrecioPorDia =
                        a.Habitacion != null &&
                        a.Habitacion.TipoHabitacion != null
                            ? a.Habitacion.TipoHabitacion.PrecioPorDia
                            : 0,
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
                .AnyAsync(p =>
                    p.IdPaciente == asignacion.IdPaciente);

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

            if (habitacion.Estado == "Mantenimiento")
            {
                return BadRequest(new
                {
                    mensaje =
                        "La habitación se encuentra en mantenimiento."
                });
            }

            if (habitacion.CantidadCamas <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La habitación no tiene camas disponibles configuradas."
                });
            }

            var asignacionesActivas =
                await _context.AsignacionesHabitacion
                    .CountAsync(a =>
                        a.IdHabitacion ==
                            asignacion.IdHabitacion &&
                        a.Estado == "Activa");

            if (asignacionesActivas >=
                habitacion.CantidadCamas)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La habitación ya tiene todas sus camas ocupadas."
                });
            }

            var pacienteYaInternado =
                await _context.AsignacionesHabitacion
                    .AnyAsync(a =>
                        a.IdPaciente ==
                            asignacion.IdPaciente &&
                        a.Estado == "Activa");

            if (pacienteYaInternado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El paciente ya tiene una asignación de habitación activa."
                });
            }

            if (asignacion.FechaIngreso == default)
            {
                asignacion.FechaIngreso = DateTime.Now;
            }

            asignacion.FechaEgreso = null;
            asignacion.Estado = "Activa";

            _context.AsignacionesHabitacion.Add(
                asignacion);

            habitacion.Estado = "Ocupada";

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "HabitacionesActualizadas");

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
                    asignacion.Estado,
                    CamasOcupadas =
                        asignacionesActivas + 1,
                    habitacion.CantidadCamas
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
                    mensaje =
                        "El ID de la URL no coincide con el ID de la asignación."
                });
            }

            var asignacionExistente =
                await _context.AsignacionesHabitacion
                    .FindAsync(id);

            if (asignacionExistente == null)
            {
                return NotFound();
            }

            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p =>
                    p.IdPaciente == asignacion.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            if (asignacion.Estado != "Activa" &&
                asignacion.Estado != "Finalizada")
            {
                return BadRequest(new
                {
                    mensaje =
                        "El estado de la asignación debe ser Activa o Finalizada."
                });
            }

            var idHabitacionAnterior =
                asignacionExistente.IdHabitacion;

            var habitacionNueva =
                await _context.Habitaciones
                    .FindAsync(asignacion.IdHabitacion);

            if (habitacionNueva == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La habitación indicada no existe."
                });
            }

            if (asignacion.Estado == "Activa")
            {
                var pacienteYaInternado =
                    await _context.AsignacionesHabitacion
                        .AnyAsync(a =>
                            a.IdPaciente ==
                                asignacion.IdPaciente &&
                            a.Estado == "Activa" &&
                            a.IdAsignacion != id);

                if (pacienteYaInternado)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El paciente ya tiene otra asignación de habitación activa."
                    });
                }

                if (habitacionNueva.CantidadCamas <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La habitación no tiene camas disponibles configuradas."
                    });
                }

                if (habitacionNueva.Estado ==
                    "Mantenimiento")
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La habitación se encuentra en mantenimiento."
                    });
                }

                var ocupadasNuevaHabitacion =
                    await _context.AsignacionesHabitacion
                        .CountAsync(a =>
                            a.IdHabitacion ==
                                asignacion.IdHabitacion &&
                            a.Estado == "Activa" &&
                            a.IdAsignacion != id);

                if (ocupadasNuevaHabitacion >=
                    habitacionNueva.CantidadCamas)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La habitación ya tiene todas sus camas ocupadas."
                    });
                }
            }

            asignacionExistente.IdPaciente =
                asignacion.IdPaciente;

            asignacionExistente.IdHabitacion =
                asignacion.IdHabitacion;

            asignacionExistente.FechaIngreso =
                asignacion.FechaIngreso;

            asignacionExistente.FechaEgreso =
                asignacion.FechaEgreso;

            asignacionExistente.Estado =
                asignacion.Estado;

            if (asignacionExistente.Estado ==
                    "Finalizada" &&
                asignacionExistente.FechaEgreso == null)
            {
                asignacionExistente.FechaEgreso =
                    DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await ActualizarEstadoHabitacion(
                idHabitacionAnterior);

            if (idHabitacionAnterior !=
                asignacion.IdHabitacion)
            {
                await ActualizarEstadoHabitacion(
                    asignacion.IdHabitacion);
            }
            else
            {
                await ActualizarEstadoHabitacion(
                    idHabitacionAnterior);
            }

            await _hubContext.Clients.All.SendAsync(
                "HabitacionesActualizadas");

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
        public async Task<IActionResult> EliminarAsignacion(
            int id)
        {
            var asignacion =
                await _context.AsignacionesHabitacion
                    .FindAsync(id);

            if (asignacion == null)
            {
                return NotFound();
            }

            var idHabitacion =
                asignacion.IdHabitacion;

            _context.AsignacionesHabitacion.Remove(
                asignacion);

            await _context.SaveChangesAsync();

            await ActualizarEstadoHabitacion(
                idHabitacion);

            await _hubContext.Clients.All.SendAsync(
                "HabitacionesActualizadas");

            return NoContent();
        }

        private async Task ActualizarEstadoHabitacion(
            int idHabitacion)
        {
            var habitacion =
                await _context.Habitaciones
                    .FindAsync(idHabitacion);

            if (habitacion == null)
            {
                return;
            }

            if (habitacion.Estado ==
                "Mantenimiento")
            {
                return;
            }

            var asignacionesActivas =
                await _context.AsignacionesHabitacion
                    .CountAsync(a =>
                        a.IdHabitacion == idHabitacion &&
                        a.Estado == "Activa");

            habitacion.Estado =
                asignacionesActivas > 0
                    ? "Ocupada"
                    : "Libre";

            await _context.SaveChangesAsync();
        }
    }
}