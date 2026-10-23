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

        public ContactosEmergenciaController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "ContactosEmergenciaConsultar")]
        public async Task<IActionResult> GetContactos()
        {
            var contactos =
                await _context.ContactosEmergencia
                    .Include(c => c.Paciente)
                    .OrderBy(c => c.IdContacto)
                    .Select(c => new
                    {
                        c.IdContacto,
                        c.IdPaciente,

                        Paciente =
                            c.Paciente != null
                                ? c.Paciente.Nombres +
                                  " " +
                                  c.Paciente.Apellidos
                                : null,

                        c.Nombre,
                        c.Parentesco,
                        c.Telefono,
                        c.Direccion
                    })
                    .ToListAsync();

            return Ok(contactos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ContactosEmergenciaConsultar")]
        public async Task<IActionResult> GetContacto(
            int id)
        {
            var contacto =
                await _context.ContactosEmergencia
                    .Include(c => c.Paciente)
                    .Where(c =>
                        c.IdContacto == id)
                    .Select(c => new
                    {
                        c.IdContacto,
                        c.IdPaciente,

                        Paciente =
                            c.Paciente != null
                                ? c.Paciente.Nombres +
                                  " " +
                                  c.Paciente.Apellidos
                                : null,

                        c.Nombre,
                        c.Parentesco,
                        c.Telefono,
                        c.Direccion
                    })
                    .FirstOrDefaultAsync();

            if (contacto == null)
            {
                return NotFound();
            }

            return Ok(contacto);
        }

        [HttpGet("paciente/{idPaciente}")]
        [Authorize(Policy = "ContactosEmergenciaConsultar")]
        public async Task<IActionResult> GetPorPaciente(
            int idPaciente)
        {
            var pacienteExiste =
                await _context.Pacientes.AnyAsync(
                    p => p.IdPaciente ==
                         idPaciente);

            if (!pacienteExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        "El paciente indicado no existe."
                });
            }

            var contactos =
                await _context.ContactosEmergencia
                    .Where(c =>
                        c.IdPaciente == idPaciente)
                    .OrderBy(c => c.IdContacto)
                    .Select(c => new
                    {
                        c.IdContacto,
                        c.IdPaciente,
                        c.Nombre,
                        c.Parentesco,
                        c.Telefono,
                        c.Direccion
                    })
                    .ToListAsync();

            return Ok(contactos);
        }

        [HttpPost]
        [Authorize(Policy = "ContactosEmergenciaCrear")]
        public async Task<IActionResult> CrearContacto(
            ContactoEmergencia contacto)
        {
            var pacienteExiste =
                await _context.Pacientes.AnyAsync(
                    p => p.IdPaciente ==
                         contacto.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El paciente indicado no existe."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    contacto.Nombre) ||
                string.IsNullOrWhiteSpace(
                    contacto.Parentesco) ||
                string.IsNullOrWhiteSpace(
                    contacto.Telefono))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Nombre, parentesco y teléfono son obligatorios."
                });
            }

            contacto.TelefonoAlternativo ??=
                string.Empty;

            contacto.Direccion ??=
                string.Empty;

            _context.ContactosEmergencia.Add(
                contacto);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetContacto),
                new
                {
                    id = contacto.IdContacto
                },
                new
                {
                    contacto.IdContacto,
                    contacto.IdPaciente,
                    contacto.Nombre,
                    contacto.Parentesco,
                    contacto.Telefono,
                    contacto.Direccion
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
                    mensaje =
                        "El ID de la URL no coincide con el ID del contacto."
                });
            }

            var existente =
                await _context.ContactosEmergencia
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var pacienteExiste =
                await _context.Pacientes.AnyAsync(
                    p => p.IdPaciente ==
                         contacto.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El paciente indicado no existe."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    contacto.Nombre) ||
                string.IsNullOrWhiteSpace(
                    contacto.Parentesco) ||
                string.IsNullOrWhiteSpace(
                    contacto.Telefono))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Nombre, parentesco y teléfono son obligatorios."
                });
            }

            existente.IdPaciente =
                contacto.IdPaciente;

            existente.Nombre =
                contacto.Nombre;

            existente.Parentesco =
                contacto.Parentesco;

            existente.Telefono =
                contacto.Telefono;

            existente.Direccion =
                contacto.Direccion ??
                string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdContacto,
                existente.IdPaciente,
                existente.Nombre,
                existente.Parentesco,
                existente.Telefono,
                existente.Direccion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ContactosEmergenciaEliminar")]
        public async Task<IActionResult> EliminarContacto(
            int id)
        {
            var contacto =
                await _context.ContactosEmergencia
                    .FindAsync(id);

            if (contacto == null)
            {
                return NotFound();
            }

            _context.ContactosEmergencia.Remove(
                contacto);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
