using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasFarmaciaController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public VentasFarmaciaController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "VentasFarmaciaConsultar")]
        public async Task<IActionResult> GetVentas()
        {
            var ventas = await _context.VentasFarmacia
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new
                {
                    v.IdVenta,
                    v.IdUsuario,
                    Usuario = v.Usuario != null
                        ? v.Usuario.UsuarioLogin
                        : null,
                    v.FechaVenta,
                    v.Total
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "VentasFarmaciaConsultar")]
        public async Task<IActionResult> GetVenta(int id)
        {
            var venta = await _context.VentasFarmacia
                .Include(v => v.Usuario)
                .Where(v => v.IdVenta == id)
                .Select(v => new
                {
                    v.IdVenta,
                    v.IdUsuario,
                    Usuario = v.Usuario != null
                        ? v.Usuario.UsuarioLogin
                        : null,
                    v.FechaVenta,
                    v.Total
                })
                .FirstOrDefaultAsync();

            if (venta == null)
            {
                return NotFound();
            }

            return Ok(venta);
        }

        [HttpPost]
        [Authorize(Policy = "VentasFarmaciaCrear")]
        public async Task<IActionResult> CrearVenta(VentaFarmacia venta)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == venta.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe."
                });
            }

            if (venta.FechaVenta == default)
            {
                venta.FechaVenta = DateTime.Now;
            }

            venta.Total = 0;

            _context.VentasFarmacia.Add(venta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetVenta),
                new { id = venta.IdVenta },
                new
                {
                    venta.IdVenta,
                    venta.IdUsuario,
                    venta.FechaVenta,
                    venta.Total
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "VentasFarmaciaModificar")]
        public async Task<IActionResult> ModificarVenta(
            int id,
            VentaFarmacia venta)
        {
            if (id != venta.IdVenta)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la venta."
                });
            }

            var ventaExistente = await _context.VentasFarmacia.FindAsync(id);

            if (ventaExistente == null)
            {
                return NotFound();
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == venta.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe."
                });
            }

            ventaExistente.IdUsuario = venta.IdUsuario;
            ventaExistente.FechaVenta = venta.FechaVenta;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ventaExistente.IdVenta,
                ventaExistente.IdUsuario,
                ventaExistente.FechaVenta,
                ventaExistente.Total
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "VentasFarmaciaEliminar")]
        public async Task<IActionResult> EliminarVenta(int id)
        {
            var venta = await _context.VentasFarmacia
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            if (venta.Detalles.Any())
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar una venta que ya tiene detalles."
                });
            }

            _context.VentasFarmacia.Remove(venta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}