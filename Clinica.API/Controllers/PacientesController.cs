using Clinica.API.Authorization;
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
            var pacientes = await _context.Pacientes.ToListAsync();

            return Ok(pacientes);
        }

        [HttpPost]
        [Authorize(Policy = "PacientesCrear")]
        public async Task<IActionResult> CrearPaciente(Paciente paciente)
        {
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPacientes),
                new { id = paciente.IdPaciente },
                paciente);
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
                    mensaje = "El ID de la URL no coincide con el ID del paciente."
                });
            }

            var pacienteExistente = await _context.Pacientes
                .FindAsync(id);

            if (pacienteExistente == null)
            {
                return NotFound();
            }

            pacienteExistente.Nombres = paciente.Nombres;
            pacienteExistente.Apellidos = paciente.Apellidos;
            pacienteExistente.DPI = paciente.DPI;
            pacienteExistente.FechaNacimiento = paciente.FechaNacimiento;
            pacienteExistente.Direccion = paciente.Direccion;
            pacienteExistente.Telefono = paciente.Telefono;
            pacienteExistente.Correo = paciente.Correo;

            await _context.SaveChangesAsync();

            return Ok(pacienteExistente);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "PacientesEliminar")]
        public async Task<IActionResult> EliminarPaciente(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
            {
                return NotFound();
            }

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}