using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactosEmergenciaController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public ContactosEmergenciaController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "ContactosEmergenciaConsultar")]
        public async Task<IActionResult> GetContactos()
        {
            var contactos = await _context.ContactosEmergencia
                .Include(c => c.Paciente)
                .OrderBy(c => c.IdContacto)
                .Select(c => new
                {
                    c.IdContacto,
                    c.IdPaciente,
                    Paciente = c.Paciente != null
                        ? c.Paciente.Nombres + " " + c.Paciente.Apellidos
                        : null,
                    c.Nombre,
                    c.Parentesco,
                    c.Telefono,
                    c.TelefonoAlternativo
                })
                .ToListAsync();

            return Ok(contactos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ContactosEmergenciaConsultar")]
        public async Task<IActionResult> GetContacto(int id)
        {
            var contacto = await _context.ContactosEmergencia
                .Include(c => c.Paciente)
                .Where(c => c.IdContacto == id)
                .Select(c => new
                {
                    c.IdContacto,
                    c.IdPaciente,
                    Paciente = c.Paciente != null
                        ? c.Paciente.Nombres + " " + c.Paciente.Apellidos
                        : null,
                    c.Nombre,
                    c.Parentesco,
                    c.Telefono,
                    c.TelefonoAlternativo
                })
                .FirstOrDefaultAsync();

            if (contacto == null)
                return NotFound();

            return Ok(contacto);
        }

        [HttpGet("paciente/{idPaciente}")]
        [Authorize(Policy = "ContactosEmergenciaConsultar")]
        public async Task<IActionResult> GetPorPaciente(int idPaciente)
        {
            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == idPaciente);

            if (!pacienteExiste)
            {
                return NotFound(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            var contactos = await _context.ContactosEmergencia
                .Where(c => c.IdPaciente == idPaciente)
                .OrderBy(c => c.IdContacto)
                .Select(c => new
                {
                    c.IdContacto,
                    c.IdPaciente,
                    c.Nombre,
                    c.Parentesco,
                    c.Telefono,
                    c.TelefonoAlternativo
                })
                .ToListAsync();

            return Ok(contactos);
        }

        [HttpPost]
        [Authorize(Policy = "ContactosEmergenciaCrear")]
        public async Task<IActionResult> CrearContacto(
            ContactoEmergencia contacto)
        {
            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == contacto.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            if (string.IsNullOrWhiteSpace(contacto.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del contacto es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(contacto.Parentesco))
            {
                return BadRequest(new
                {
                    mensaje = "El parentesco es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                return BadRequest(new
                {
                    mensaje = "El teléfono es obligatorio."
                });
            }

            _context.ContactosEmergencia.Add(contacto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetContacto),
                new { id = contacto.IdContacto },
                new
                {
                    contacto.IdContacto,
                    contacto.IdPaciente,
                    contacto.Nombre,
                    contacto.Parentesco,
                    contacto.Telefono,
                    contacto.TelefonoAlternativo
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ContactosEmergenciaModificar")]
        public async Task<IActionResult> ModificarContacto(
            int id,
            ContactoEmergencia contacto)
        {
            if (id != contacto.IdContacto)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del contacto."
                });
            }

            var contactoExistente = await _context.ContactosEmergencia
                .FindAsync(id);

            if (contactoExistente == null)
                return NotFound();

            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == contacto.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            if (string.IsNullOrWhiteSpace(contacto.Nombre) ||
                string.IsNullOrWhiteSpace(contacto.Parentesco) ||
                string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                return BadRequest(new
                {
                    mensaje = "Nombre, parentesco y teléfono son obligatorios."
                });
            }

            contactoExistente.IdPaciente = contacto.IdPaciente;
            contactoExistente.Nombre = contacto.Nombre;
            contactoExistente.Parentesco = contacto.Parentesco;
            contactoExistente.Telefono = contacto.Telefono;
            contactoExistente.TelefonoAlternativo =
                contacto.TelefonoAlternativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                contactoExistente.IdContacto,
                contactoExistente.IdPaciente,
                contactoExistente.Nombre,
                contactoExistente.Parentesco,
                contactoExistente.Telefono,
                contactoExistente.TelefonoAlternativo
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ContactosEmergenciaEliminar")]
        public async Task<IActionResult> EliminarContacto(int id)
        {
            var contacto = await _context.ContactosEmergencia
                .FindAsync(id);

            if (contacto == null)
                return NotFound();

            _context.ContactosEmergencia.Remove(contacto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}