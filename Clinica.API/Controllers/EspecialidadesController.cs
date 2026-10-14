using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspecialidadesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public EspecialidadesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "EspecialidadesConsultar")]
        public async Task<IActionResult> GetEspecialidades()
        {
            var especialidades = await _context.Especialidades
                .OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    e.IdEspecialidad,
                    e.Nombre,
                    e.Descripcion,
                    CantidadEmpleados = e.Empleados.Count
                })
                .ToListAsync();

            return Ok(especialidades);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "EspecialidadesConsultar")]
        public async Task<IActionResult> GetEspecialidad(int id)
        {
            var especialidad = await _context.Especialidades
                .Where(e => e.IdEspecialidad == id)
                .Select(e => new
                {
                    e.IdEspecialidad,
                    e.Nombre,
                    e.Descripcion,
                    CantidadEmpleados = e.Empleados.Count
                })
                .FirstOrDefaultAsync();

            if (especialidad == null)
            {
                return NotFound();
            }

            return Ok(especialidad);
        }

        [HttpPost]
        [Authorize(Policy = "EspecialidadesCrear")]
        public async Task<IActionResult> CrearEspecialidad(
            Especialidad especialidad)
        {
            if (string.IsNullOrWhiteSpace(especialidad.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la especialidad es obligatorio."
                });
            }

            var nombre = especialidad.Nombre.Trim();

            var existe = await _context.Especialidades
                .AnyAsync(e => e.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe una especialidad con ese nombre."
                });
            }

            especialidad.Nombre = nombre;
            especialidad.Descripcion =
                especialidad.Descripcion?.Trim() ?? string.Empty;

            _context.Especialidades.Add(especialidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEspecialidad),
                new { id = especialidad.IdEspecialidad },
                new
                {
                    especialidad.IdEspecialidad,
                    especialidad.Nombre,
                    especialidad.Descripcion
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "EspecialidadesModificar")]
        public async Task<IActionResult> ModificarEspecialidad(
            int id,
            Especialidad especialidad)
        {
            if (id != especialidad.IdEspecialidad)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la especialidad."
                });
            }

            var existente = await _context.Especialidades
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(especialidad.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la especialidad es obligatorio."
                });
            }

            var nombre = especialidad.Nombre.Trim();

            var nombreExiste = await _context.Especialidades
                .AnyAsync(e =>
                    e.Nombre == nombre &&
                    e.IdEspecialidad != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otra especialidad con ese nombre."
                });
            }

            existente.Nombre = nombre;
            existente.Descripcion =
                especialidad.Descripcion?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdEspecialidad,
                existente.Nombre,
                existente.Descripcion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "EspecialidadesEliminar")]
        public async Task<IActionResult> EliminarEspecialidad(int id)
        {
            var especialidad = await _context.Especialidades
                .FindAsync(id);

            if (especialidad == null)
            {
                return NotFound();
            }

            var tieneEmpleados = await _context.Empleados
                .AnyAsync(e => e.IdEspecialidad == id);

            if (tieneEmpleados)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la especialidad porque tiene empleados asociados."
                });
            }

            _context.Especialidades.Remove(especialidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}