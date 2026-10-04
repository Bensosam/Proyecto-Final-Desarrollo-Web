using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabitacionesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public HabitacionesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "HabitacionesConsultar")]
        public async Task<IActionResult> GetHabitaciones()
        {
            var habitaciones = await _context.Habitaciones
                .Include(h => h.Sucursal)
                .OrderBy(h => h.Numero)
                .Select(h => new
                {
                    h.IdHabitacion,
                    h.IdSucursal,
                    Sucursal = h.Sucursal != null
                        ? h.Sucursal.Nombre
                        : null,
                    h.Numero,
                    h.Estado
                })
                .ToListAsync();

            return Ok(habitaciones);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "HabitacionesConsultar")]
        public async Task<IActionResult> GetHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones
                .Include(h => h.Sucursal)
                .Where(h => h.IdHabitacion == id)
                .Select(h => new
                {
                    h.IdHabitacion,
                    h.IdSucursal,
                    Sucursal = h.Sucursal != null
                        ? h.Sucursal.Nombre
                        : null,
                    h.Numero,
                    h.Estado
                })
                .FirstOrDefaultAsync();

            if (habitacion == null)
            {
                return NotFound();
            }

            return Ok(habitacion);
        }

        [HttpPost]
        [Authorize(Policy = "HabitacionesCrear")]
        public async Task<IActionResult> CrearHabitacion(Habitacion habitacion)
        {
            var sucursalExiste = await _context.Sucursales
                .AnyAsync(s => s.IdSucursal == habitacion.IdSucursal);

            if (!sucursalExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La sucursal indicada no existe."
                });
            }

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetHabitacion),
                new { id = habitacion.IdHabitacion },
                new
                {
                    habitacion.IdHabitacion,
                    habitacion.IdSucursal,
                    habitacion.Numero,
                    habitacion.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "HabitacionesModificar")]
        public async Task<IActionResult> ModificarHabitacion(
            int id,
            Habitacion habitacion)
        {
            if (id != habitacion.IdHabitacion)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la habitación."
                });
            }

            var habitacionExistente = await _context.Habitaciones
                .FindAsync(id);

            if (habitacionExistente == null)
            {
                return NotFound();
            }

            var sucursalExiste = await _context.Sucursales
                .AnyAsync(s => s.IdSucursal == habitacion.IdSucursal);

            if (!sucursalExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La sucursal indicada no existe."
                });
            }

            habitacionExistente.IdSucursal = habitacion.IdSucursal;
            habitacionExistente.Numero = habitacion.Numero;
            habitacionExistente.Estado = habitacion.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                habitacionExistente.IdHabitacion,
                habitacionExistente.IdSucursal,
                habitacionExistente.Numero,
                habitacionExistente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "HabitacionesEliminar")]
        public async Task<IActionResult> EliminarHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones
                .FindAsync(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
