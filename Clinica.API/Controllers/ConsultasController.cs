using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public ConsultasController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: api/Consultas
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "ConsultasConsultar")]
        public async Task<IActionResult> GetConsultas()
        {
            var consultas =
                await _context.Consultas
                    .Include(c => c.Paciente)
                    .Include(c => c.Empleado)
                    .OrderByDescending(c => c.FechaConsulta)
                    .Select(c => new
                    {
                        c.IdConsulta,
                        c.IdPaciente,
                        c.NumeroConsultaPaciente,

                        Paciente =
                            c.Paciente != null
                                ? c.Paciente.Nombres +
                                  " " +
                                  c.Paciente.Apellidos
                                : null,

                        c.IdEmpleado,

                        Empleado =
                            c.Empleado != null
                                ? c.Empleado.Nombres +
                                  " " +
                                  c.Empleado.Apellidos
                                : null,

                        c.FechaConsulta,
                        c.Motivo,
                        c.Observaciones
                    })
                    .ToListAsync();

            return Ok(consultas);
        }

        // =========================================================
        // GET: api/Consultas/1
        // =========================================================

        [HttpGet("{id}")]
        [Authorize(Policy = "ConsultasConsultar")]
        public async Task<IActionResult> GetConsulta(
            int id)
        {
            var consulta =
                await _context.Consultas
                    .Include(c => c.Paciente)
                    .Include(c => c.Empleado)
                    .Where(c =>
                        c.IdConsulta == id)
                    .Select(c => new
                    {
                        c.IdConsulta,
                        c.IdPaciente,
                        c.NumeroConsultaPaciente,

                        Paciente =
                            c.Paciente != null
                                ? c.Paciente.Nombres +
                                  " " +
                                  c.Paciente.Apellidos
                                : null,

                        c.IdEmpleado,

                        Empleado =
                            c.Empleado != null
                                ? c.Empleado.Nombres +
                                  " " +
                                  c.Empleado.Apellidos
                                : null,

                        c.FechaConsulta,
                        c.Motivo,
                        c.Observaciones
                    })
                    .FirstOrDefaultAsync();

            if (consulta == null)
            {
                return NotFound();
            }

            return Ok(consulta);
        }

        // =========================================================
        // POST: api/Consultas
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "ConsultasCrear")]
        public async Task<IActionResult> CrearConsulta(
            Consulta consulta)
        {
            var paciente =
                await _context.Pacientes
                    .Where(p =>
                        p.IdPaciente ==
                        consulta.IdPaciente)
                    .Select(p => new
                    {
                        p.IdPaciente,
                        p.Estado
                    })
                    .FirstOrDefaultAsync();

            if (paciente == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El paciente indicado no existe."
                });
            }

            if (!paciente.Estado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede registrar una consulta para un paciente inactivo."
                });
            }

            var empleadoExiste =
                await _context.Empleados
                    .AnyAsync(e =>
                        e.IdEmpleado ==
                        consulta.IdEmpleado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El empleado indicado no existe."
                });
            }

            // Obtener el último número de consulta
            // registrado PARA ESTE PACIENTE.
            var ultimoNumero =
                await _context.Consultas
                    .Where(c =>
                        c.IdPaciente ==
                        consulta.IdPaciente)
                    .MaxAsync(c =>
                        (int?)
                        c.NumeroConsultaPaciente)
                ?? 0;

            // El usuario NO decide este número.
            consulta.NumeroConsultaPaciente =
                ultimoNumero + 1;

            if (consulta.FechaConsulta == default)
            {
                consulta.FechaConsulta =
                    DateTime.Now;
            }

            _context.Consultas.Add(
                consulta);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetConsulta),
                new
                {
                    id =
                        consulta.IdConsulta
                },
                new
                {
                    consulta.IdConsulta,
                    consulta.IdPaciente,
                    consulta.NumeroConsultaPaciente,
                    consulta.IdEmpleado,
                    consulta.FechaConsulta,
                    consulta.Motivo,
                    consulta.Observaciones
                });
        }

        // =========================================================
        // PUT: api/Consultas/1
        // =========================================================

        [HttpPut("{id}")]
        [Authorize(Policy = "ConsultasModificar")]
        public async Task<IActionResult> ModificarConsulta(
            int id,
            Consulta consulta)
        {
            if (id != consulta.IdConsulta)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID de la consulta."
                });
            }

            var consultaExistente =
                await _context.Consultas
                    .FindAsync(id);

            if (consultaExistente == null)
            {
                return NotFound();
            }

            // Una consulta ya registrada no debe cambiarse
            // posteriormente a otro paciente porque rompería
            // la numeración histórica.
            if (consultaExistente.IdPaciente !=
                consulta.IdPaciente)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede cambiar el paciente de una consulta ya registrada."
                });
            }

            var empleadoExiste =
                await _context.Empleados
                    .AnyAsync(e =>
                        e.IdEmpleado ==
                        consulta.IdEmpleado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El empleado indicado no existe."
                });
            }

            consultaExistente.IdEmpleado =
                consulta.IdEmpleado;

            consultaExistente.FechaConsulta =
                consulta.FechaConsulta;

            consultaExistente.Motivo =
                consulta.Motivo;

            consultaExistente.Observaciones =
                consulta.Observaciones;

            // NumeroConsultaPaciente NO se modifica.

            await _context.SaveChangesAsync();

            return Ok(new
            {
                consultaExistente.IdConsulta,
                consultaExistente.IdPaciente,
                consultaExistente.NumeroConsultaPaciente,
                consultaExistente.IdEmpleado,
                consultaExistente.FechaConsulta,
                consultaExistente.Motivo,
                consultaExistente.Observaciones
            });
        }

        // =========================================================
        // DELETE: api/Consultas/1
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize(Policy = "ConsultasEliminar")]
        public async Task<IActionResult> EliminarConsulta(
            int id)
        {
            var consulta =
                await _context.Consultas
                    .FindAsync(id);

            if (consulta == null)
            {
                return NotFound();
            }

            _context.Consultas.Remove(
                consulta);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}