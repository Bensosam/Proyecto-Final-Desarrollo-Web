using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SucursalesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public SucursalesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "SucursalesConsultar")]
        public async Task<IActionResult> GetSucursales()
        {
            var sucursales = await _context.Sucursales
                .OrderBy(s => s.Nombre)
                .Select(s => new
                {
                    s.IdSucursal,
                    s.Nombre,
                    s.Direccion,
                    s.Telefono,
                    s.Estado,
                    CantidadHabitaciones = s.Habitaciones.Count,
                    CantidadEmpleados = s.Empleados.Count
                })
                .ToListAsync();

            return Ok(sucursales);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "SucursalesConsultar")]
        public async Task<IActionResult> GetSucursal(int id)
        {
            var sucursal = await _context.Sucursales
                .Where(s => s.IdSucursal == id)
                .Select(s => new
                {
                    s.IdSucursal,
                    s.Nombre,
                    s.Direccion,
                    s.Telefono,
                    s.Estado,
                    CantidadHabitaciones = s.Habitaciones.Count,
                    CantidadEmpleados = s.Empleados.Count
                })
                .FirstOrDefaultAsync();

            if (sucursal == null)
            {
                return NotFound();
            }

            return Ok(sucursal);
        }

        [HttpPost]
        [Authorize(Policy = "SucursalesCrear")]
        public async Task<IActionResult> CrearSucursal(
            Sucursal sucursal)
        {
            if (string.IsNullOrWhiteSpace(sucursal.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la sucursal es obligatorio."
                });
            }

            var nombre = sucursal.Nombre.Trim();

            var existe = await _context.Sucursales
                .AnyAsync(s => s.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe una sucursal con ese nombre."
                });
            }

            sucursal.Nombre = nombre;
            sucursal.Direccion =
                sucursal.Direccion?.Trim() ?? string.Empty;
            sucursal.Telefono =
                sucursal.Telefono?.Trim() ?? string.Empty;

            _context.Sucursales.Add(sucursal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSucursal),
                new { id = sucursal.IdSucursal },
                new
                {
                    sucursal.IdSucursal,
                    sucursal.Nombre,
                    sucursal.Direccion,
                    sucursal.Telefono,
                    sucursal.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "SucursalesModificar")]
        public async Task<IActionResult> ModificarSucursal(
            int id,
            Sucursal sucursal)
        {
            if (id != sucursal.IdSucursal)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la sucursal."
                });
            }

            var existente = await _context.Sucursales
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(sucursal.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la sucursal es obligatorio."
                });
            }

            var nombre = sucursal.Nombre.Trim();

            var nombreExiste = await _context.Sucursales
                .AnyAsync(s =>
                    s.Nombre == nombre &&
                    s.IdSucursal != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otra sucursal con ese nombre."
                });
            }

            existente.Nombre = nombre;
            existente.Direccion =
                sucursal.Direccion?.Trim() ?? string.Empty;
            existente.Telefono =
                sucursal.Telefono?.Trim() ?? string.Empty;
            existente.Estado = sucursal.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdSucursal,
                existente.Nombre,
                existente.Direccion,
                existente.Telefono,
                existente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "SucursalesEliminar")]
        public async Task<IActionResult> EliminarSucursal(int id)
        {
            var sucursal = await _context.Sucursales
                .FindAsync(id);

            if (sucursal == null)
            {
                return NotFound();
            }

            var tieneHabitaciones = await _context.Habitaciones
                .AnyAsync(h => h.IdSucursal == id);

            if (tieneHabitaciones)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la sucursal porque tiene habitaciones asociadas."
                });
            }

            var tieneEmpleados = await _context.Empleados
                .AnyAsync(e => e.IdSucursal == id);

            if (tieneEmpleados)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la sucursal porque tiene empleados asociados."
                });
            }

            _context.Sucursales.Remove(sucursal);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}