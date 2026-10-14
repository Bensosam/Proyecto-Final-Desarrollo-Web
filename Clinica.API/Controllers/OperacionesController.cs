using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperacionesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        private static readonly string[] OperacionesBase =
        {
            "Consultar",
            "Crear",
            "Modificar",
            "Eliminar"
        };

        public OperacionesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "OperacionesConsultar")]
        public async Task<IActionResult> GetOperaciones()
        {
            var operaciones = await _context.Operaciones
                .OrderBy(o => o.Nombre)
                .Select(o => new
                {
                    o.IdOperacion,
                    o.Nombre,
                    CantidadPermisos = o.RolPermisos.Count
                })
                .ToListAsync();

            return Ok(operaciones);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "OperacionesConsultar")]
        public async Task<IActionResult> GetOperacion(int id)
        {
            var operacion = await _context.Operaciones
                .Where(o => o.IdOperacion == id)
                .Select(o => new
                {
                    o.IdOperacion,
                    o.Nombre,
                    CantidadPermisos = o.RolPermisos.Count
                })
                .FirstOrDefaultAsync();

            if (operacion == null)
            {
                return NotFound();
            }

            return Ok(operacion);
        }

        [HttpPost]
        [Authorize(Policy = "OperacionesCrear")]
        public async Task<IActionResult> CrearOperacion(
            Operacion operacion)
        {
            if (string.IsNullOrWhiteSpace(operacion.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la operación es obligatorio."
                });
            }

            var nombre = operacion.Nombre.Trim();

            var existe = await _context.Operaciones
                .AnyAsync(o => o.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe una operación con ese nombre."
                });
            }

            operacion.Nombre = nombre;

            _context.Operaciones.Add(operacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOperacion),
                new { id = operacion.IdOperacion },
                new
                {
                    operacion.IdOperacion,
                    operacion.Nombre
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "OperacionesModificar")]
        public async Task<IActionResult> ModificarOperacion(
            int id,
            Operacion operacion)
        {
            if (id != operacion.IdOperacion)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la operación."
                });
            }

            var existente = await _context.Operaciones
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(operacion.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la operación es obligatorio."
                });
            }

            var nombre = operacion.Nombre.Trim();

            var nombreExiste = await _context.Operaciones
                .AnyAsync(o =>
                    o.Nombre == nombre &&
                    o.IdOperacion != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otra operación con ese nombre."
                });
            }

            if (OperacionesBase.Contains(existente.Nombre) &&
                existente.Nombre != nombre)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede cambiar el nombre de una operación base del sistema."
                });
            }

            var tienePermisos = await _context.RolPermisos
                .AnyAsync(rp => rp.IdOperacion == id);

            if (tienePermisos &&
                existente.Nombre != nombre)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede cambiar el nombre de una operación que ya tiene permisos asignados."
                });
            }

            existente.Nombre = nombre;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdOperacion,
                existente.Nombre
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "OperacionesEliminar")]
        public async Task<IActionResult> EliminarOperacion(int id)
        {
            var operacion = await _context.Operaciones
                .FindAsync(id);

            if (operacion == null)
            {
                return NotFound();
            }

            if (OperacionesBase.Contains(operacion.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar una operación base del sistema."
                });
            }

            var tienePermisos = await _context.RolPermisos
                .AnyAsync(rp => rp.IdOperacion == id);

            if (tienePermisos)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la operación porque tiene permisos asignados."
                });
            }

            _context.Operaciones.Remove(operacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}