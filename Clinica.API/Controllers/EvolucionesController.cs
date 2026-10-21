using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvolucionesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public EvolucionesController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "EvolucionesConsultar")]
        public async Task<IActionResult> GetEvoluciones()
        {
            var evoluciones =
                await _context.Evoluciones
                    .OrderByDescending(e => e.Fecha)
                    .Select(e => new
                    {
                        e.IdEvolucion,
                        e.IdConsulta,
                        e.IdReceta,
                        e.IdTratamiento,

                        IdPaciente =
                            e.Consulta != null
                                ? e.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            e.Consulta != null &&
                            e.Consulta.Paciente != null
                                ? e.Consulta.Paciente.Nombres +
                                  " " +
                                  e.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            e.Consulta != null
                                ? e.Consulta.NumeroConsultaPaciente
                                : 0,

                        Receta =
                            e.Receta != null
                                ? "Receta #" +
                                  e.Receta.IdReceta
                                : null,

                        Tratamiento =
                            e.Tratamiento != null
                                ? e.Tratamiento.Descripcion
                                : null,

                        e.IdEmpleado,

                        Empleado =
                            e.Empleado != null
                                ? e.Empleado.Nombres +
                                  " " +
                                  e.Empleado.Apellidos
                                : null,

                        e.Fecha,
                        e.Descripcion
                    })
                    .ToListAsync();

            return Ok(evoluciones);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "EvolucionesConsultar")]
        public async Task<IActionResult> GetEvolucion(
            int id)
        {
            var evolucion =
                await _context.Evoluciones
                    .Where(e =>
                        e.IdEvolucion == id)
                    .Select(e => new
                    {
                        e.IdEvolucion,
                        e.IdConsulta,
                        e.IdReceta,
                        e.IdTratamiento,

                        IdPaciente =
                            e.Consulta != null
                                ? e.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            e.Consulta != null &&
                            e.Consulta.Paciente != null
                                ? e.Consulta.Paciente.Nombres +
                                  " " +
                                  e.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            e.Consulta != null
                                ? e.Consulta.NumeroConsultaPaciente
                                : 0,

                        Receta =
                            e.Receta != null
                                ? "Receta #" +
                                  e.Receta.IdReceta
                                : null,

                        Tratamiento =
                            e.Tratamiento != null
                                ? e.Tratamiento.Descripcion
                                : null,

                        e.IdEmpleado,

                        Empleado =
                            e.Empleado != null
                                ? e.Empleado.Nombres +
                                  " " +
                                  e.Empleado.Apellidos
                                : null,

                        e.Fecha,
                        e.Descripcion
                    })
                    .FirstOrDefaultAsync();

            if (evolucion == null)
            {
                return NotFound();
            }

            return Ok(evolucion);
        }

        [HttpPost]
        [Authorize(Policy = "EvolucionesCrear")]
        public async Task<IActionResult> CrearEvolucion(
            Evolucion evolucion)
        {
            if (evolucion.Fecha == default)
            {
                evolucion.Fecha =
                    DateTime.Now;
            }

            var origen =
                await ObtenerConsultaOrigen(
                    evolucion.IdReceta,
                    evolucion.IdTratamiento);

            if (origen.Error != null)
            {
                return BadRequest(new
                {
                    mensaje = origen.Error
                });
            }

            var empleadoExiste =
                await _context.Empleados
                    .AnyAsync(e =>
                        e.IdEmpleado ==
                        evolucion.IdEmpleado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El empleado indicado no existe."
                });
            }

            var errorFecha =
                await ValidarFechaEvolucion(
                    evolucion.IdReceta,
                    evolucion.IdTratamiento,
                    evolucion.Fecha);

            if (errorFecha != null)
            {
                return BadRequest(new
                {
                    mensaje = errorFecha
                });
            }

            evolucion.IdConsulta =
                origen.IdConsulta!.Value;

            _context.Evoluciones.Add(
                evolucion);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvolucion),
                new
                {
                    id = evolucion.IdEvolucion
                },
                new
                {
                    evolucion.IdEvolucion,
                    evolucion.IdConsulta,
                    evolucion.IdReceta,
                    evolucion.IdTratamiento,
                    evolucion.IdEmpleado,
                    evolucion.Fecha,
                    evolucion.Descripcion
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "EvolucionesModificar")]
        public async Task<IActionResult> ModificarEvolucion(
            int id,
            Evolucion evolucion)
        {
            if (id != evolucion.IdEvolucion)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID de la evolución."
                });
            }

            var existente =
                await _context.Evoluciones
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var origen =
                await ObtenerConsultaOrigen(
                    evolucion.IdReceta,
                    evolucion.IdTratamiento);

            if (origen.Error != null)
            {
                return BadRequest(new
                {
                    mensaje = origen.Error
                });
            }

            var empleadoExiste =
                await _context.Empleados
                    .AnyAsync(e =>
                        e.IdEmpleado ==
                        evolucion.IdEmpleado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El empleado indicado no existe."
                });
            }

            var errorFecha =
                await ValidarFechaEvolucion(
                    evolucion.IdReceta,
                    evolucion.IdTratamiento,
                    evolucion.Fecha);

            if (errorFecha != null)
            {
                return BadRequest(new
                {
                    mensaje = errorFecha
                });
            }

            existente.IdConsulta =
                origen.IdConsulta!.Value;

            existente.IdReceta =
                evolucion.IdReceta;

            existente.IdTratamiento =
                evolucion.IdTratamiento;

            existente.IdEmpleado =
                evolucion.IdEmpleado;

            existente.Fecha =
                evolucion.Fecha;

            existente.Descripcion =
                evolucion.Descripcion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdEvolucion,
                existente.IdConsulta,
                existente.IdReceta,
                existente.IdTratamiento,
                existente.IdEmpleado,
                existente.Fecha,
                existente.Descripcion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "EvolucionesEliminar")]
        public async Task<IActionResult> EliminarEvolucion(
            int id)
        {
            var evolucion =
                await _context.Evoluciones
                    .FindAsync(id);

            if (evolucion == null)
            {
                return NotFound();
            }

            _context.Evoluciones.Remove(
                evolucion);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<(int? IdConsulta, string? Error)>
            ObtenerConsultaOrigen(
                int? idReceta,
                int? idTratamiento)
        {
            if (!idReceta.HasValue &&
                !idTratamiento.HasValue)
            {
                return (
                    null,
                    "Debe seleccionar una receta, un tratamiento o ambos.");
            }

            int? consultaReceta = null;
            int? consultaTratamiento = null;

            if (idReceta.HasValue)
            {
                var receta =
                    await _context.Recetas
                        .Where(r =>
                            r.IdReceta ==
                            idReceta.Value)
                        .Select(r => new
                        {
                            r.IdConsulta
                        })
                        .FirstOrDefaultAsync();

                if (receta == null)
                {
                    return (
                        null,
                        "La receta indicada no existe.");
                }

                consultaReceta =
                    receta.IdConsulta;
            }

            if (idTratamiento.HasValue)
            {
                var tratamiento =
                    await _context.Tratamientos
                        .Where(t =>
                            t.IdTratamiento ==
                            idTratamiento.Value)
                        .Select(t => new
                        {
                            t.IdConsulta
                        })
                        .FirstOrDefaultAsync();

                if (tratamiento == null)
                {
                    return (
                        null,
                        "El tratamiento indicado no existe.");
                }

                consultaTratamiento =
                    tratamiento.IdConsulta;
            }

            if (consultaReceta.HasValue &&
                consultaTratamiento.HasValue &&
                consultaReceta.Value !=
                consultaTratamiento.Value)
            {
                return (
                    null,
                    "La receta y el tratamiento deben pertenecer a la misma consulta.");
            }

            return (
                consultaReceta ??
                consultaTratamiento,
                null);
        }

        private async Task<string?>
            ValidarFechaEvolucion(
                int? idReceta,
                int? idTratamiento,
                DateTime fechaEvolucion)
        {
            DateTime? fechaMinima = null;

            // Si existe receta, la evolución tampoco
            // puede ser anterior a su emisión.
            if (idReceta.HasValue)
            {
                var fechaEmision =
                    await _context.Recetas
                        .Where(r =>
                            r.IdReceta ==
                            idReceta.Value)
                        .Select(r =>
                            (DateTime?)
                            r.FechaEmision)
                        .FirstOrDefaultAsync();

                if (fechaEmision.HasValue)
                {
                    fechaMinima =
                        fechaEmision.Value.Date;
                }
            }

            // Si existe tratamiento, la evolución solo puede
            // registrarse al finalizar dicho tratamiento.
            if (idTratamiento.HasValue)
            {
                var tratamiento =
                    await _context.Tratamientos
                        .Where(t =>
                            t.IdTratamiento ==
                            idTratamiento.Value)
                        .Select(t => new
                        {
                            t.FechaInicio,
                            t.FechaFin
                        })
                        .FirstOrDefaultAsync();

                if (tratamiento == null)
                {
                    return
                        "El tratamiento indicado no existe.";
                }

                if (!tratamiento.FechaFin.HasValue)
                {
                    return
                        "No se puede registrar una evolución porque el tratamiento todavía no tiene fecha de finalización.";
                }

                var fechaFin =
                    tratamiento.FechaFin.Value.Date;

                if (!fechaMinima.HasValue ||
                    fechaFin >
                    fechaMinima.Value)
                {
                    fechaMinima =
                        fechaFin;
                }
            }

            if (fechaMinima.HasValue &&
                fechaEvolucion.Date <
                fechaMinima.Value.Date)
            {
                return
                    $"La evolución no puede registrarse antes del {fechaMinima.Value:dd/MM/yyyy}, fecha en que finaliza el tratamiento relacionado.";
            }

            return null;
        }
    }
}