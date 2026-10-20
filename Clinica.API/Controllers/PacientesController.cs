using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public PacientesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "PacientesConsultar")]
        public async Task<IActionResult> GetPacientes()
        {
            var pacientes = await _context.Pacientes
                .OrderBy(p => p.IdPaciente)
                .ToListAsync();

            return Ok(pacientes);
        }

        [HttpGet("activos")]
        [Authorize]
        public async Task<IActionResult> GetPacientesActivos()
        {
            var pacientes = await _context.Pacientes
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombres)
                .ThenBy(p => p.Apellidos)
                .Select(p => new
                {
                    p.IdPaciente,
                    p.Nombres,
                    p.Apellidos
                })
                .ToListAsync();

            return Ok(pacientes);
        }

        [HttpPost]
        [Authorize(Policy = "PacientesCrear")]
        public async Task<IActionResult> CrearPaciente(
            Paciente paciente)
        {
            var generoNormalizado =
                NormalizarGenero(paciente.Genero);

            if (generoNormalizado == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El género debe ser Masculino o Femenino."
                });
            }

            paciente.Genero = generoNormalizado;
            paciente.FechaRegistro = DateTime.Now;
            paciente.Estado = true;

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            return Ok(paciente);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "PacientesModificar")]
        public async Task<IActionResult> ModificarPaciente(
            int id,
            Paciente paciente)
        {
            if (id != paciente.IdPaciente)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del paciente."
                });
            }

            var generoNormalizado =
                NormalizarGenero(paciente.Genero);

            if (generoNormalizado == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El género debe ser Masculino o Femenino."
                });
            }

            var pacienteExistente =
                await _context.Pacientes
                    .FindAsync(id);

            if (pacienteExistente == null)
            {
                return NotFound();
            }

            if (pacienteExistente.Estado &&
                !paciente.Estado)
            {
                var tieneHospitalizacionActiva =
                    await _context.AsignacionesHabitacion
                        .AnyAsync(a =>
                            a.IdPaciente == id &&
                            a.Estado == "Activa");

                if (tieneHospitalizacionActiva)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "No se puede inactivar al paciente porque tiene una hospitalización activa. Finalice primero la hospitalización."
                    });
                }
            }

            pacienteExistente.Nombres =
                paciente.Nombres;

            pacienteExistente.Apellidos =
                paciente.Apellidos;

            pacienteExistente.DPI =
                paciente.DPI;

            pacienteExistente.FechaNacimiento =
                paciente.FechaNacimiento;

            pacienteExistente.Genero =
                generoNormalizado;

            pacienteExistente.Direccion =
                paciente.Direccion;

            pacienteExistente.Telefono =
                paciente.Telefono;

            pacienteExistente.Correo =
                paciente.Correo;

            pacienteExistente.Estado =
                paciente.Estado;

            await _context.SaveChangesAsync();

            return Ok(pacienteExistente);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "PacientesEliminar")]
        public async Task<IActionResult> EliminarPaciente(
            int id)
        {
            var paciente =
                await _context.Pacientes
                    .FindAsync(id);

            if (paciente == null)
            {
                return NotFound();
            }

            var tieneHospitalizacionActiva =
                await _context.AsignacionesHabitacion
                    .AnyAsync(a =>
                        a.IdPaciente == id &&
                        a.Estado == "Activa");

            if (tieneHospitalizacionActiva)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar al paciente porque tiene una hospitalización activa. Finalice primero la hospitalización."
                });
            }

            _context.Pacientes.Remove(paciente);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string? NormalizarGenero(
            string? genero)
        {
            if (string.Equals(
                    genero,
                    "Masculino",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Masculino";
            }

            if (string.Equals(
                    genero,
                    "Femenino",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Femenino";
            }

            return null;
        }
    }
}