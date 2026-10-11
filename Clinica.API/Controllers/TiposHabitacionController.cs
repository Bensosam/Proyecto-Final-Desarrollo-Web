using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposHabitacionController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public TiposHabitacionController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "TiposHabitacionConsultar")]
        public async Task<IActionResult> GetTiposHabitacion()
        {
            var tipos = await _context.TiposHabitacion
                .OrderBy(t => t.Nombre)
                .Select(t => new
                {
                    t.IdTipoHabitacion,
                    t.Nombre,
                    t.Descripcion,
                    t.PrecioPorDia,
                    t.Estado
                })
                .ToListAsync();

            return Ok(tipos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "TiposHabitacionConsultar")]
        public async Task<IActionResult> GetTipoHabitacion(int id)
        {
            var tipo = await _context.TiposHabitacion
                .Where(t => t.IdTipoHabitacion == id)
                .Select(t => new
                {
                    t.IdTipoHabitacion,
                    t.Nombre,
                    t.Descripcion,
                    t.PrecioPorDia,
                    t.Estado
                })
                .FirstOrDefaultAsync();

            if (tipo == null)
            {
                return NotFound();
            }

            return Ok(tipo);
        }

        [HttpPost]
        [Authorize(Policy = "TiposHabitacionCrear")]
        public async Task<IActionResult> CrearTipoHabitacion(
            TipoHabitacion tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del tipo de habitación es obligatorio."
                });
            }

            if (tipo.PrecioPorDia < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio por día no puede ser negativo."
                });
            }

            var nombreExiste = await _context.TiposHabitacion
                .AnyAsync(t => t.Nombre == tipo.Nombre);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe un tipo de habitación con ese nombre."
                });
            }

            _context.TiposHabitacion.Add(tipo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTipoHabitacion),
                new { id = tipo.IdTipoHabitacion },
                new
                {
                    tipo.IdTipoHabitacion,
                    tipo.Nombre,
                    tipo.Descripcion,
                    tipo.PrecioPorDia,
                    tipo.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "TiposHabitacionModificar")]
        public async Task<IActionResult> ModificarTipoHabitacion(
            int id,
            TipoHabitacion tipo)
        {
            if (id != tipo.IdTipoHabitacion)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del tipo de habitación."
                });
            }

            var tipoExistente = await _context.TiposHabitacion
                .FindAsync(id);

            if (tipoExistente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(tipo.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del tipo de habitación es obligatorio."
                });
            }

            if (tipo.PrecioPorDia < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio por día no puede ser negativo."
                });
            }

            var nombreExiste = await _context.TiposHabitacion
                .AnyAsync(t =>
                    t.Nombre == tipo.Nombre &&
                    t.IdTipoHabitacion != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otro tipo de habitación con ese nombre."
                });
            }

            tipoExistente.Nombre = tipo.Nombre;
            tipoExistente.Descripcion = tipo.Descripcion;
            tipoExistente.PrecioPorDia = tipo.PrecioPorDia;
            tipoExistente.Estado = tipo.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                tipoExistente.IdTipoHabitacion,
                tipoExistente.Nombre,
                tipoExistente.Descripcion,
                tipoExistente.PrecioPorDia,
                tipoExistente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "TiposHabitacionEliminar")]
        public async Task<IActionResult> EliminarTipoHabitacion(int id)
        {
            var tipo = await _context.TiposHabitacion
                .FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            var tieneHabitaciones = await _context.Habitaciones
                .AnyAsync(h => h.IdTipoHabitacion == id);

            if (tieneHabitaciones)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar un tipo de habitación que está siendo utilizado."
                });
            }

            _context.TiposHabitacion.Remove(tipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}