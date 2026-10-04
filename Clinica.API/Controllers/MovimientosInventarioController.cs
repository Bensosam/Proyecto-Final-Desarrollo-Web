using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosInventarioController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public MovimientosInventarioController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "MovimientosInventarioConsultar")]
        public async Task<IActionResult> GetMovimientos()
        {
            var movimientos = await _context.MovimientosInventario
                .Include(m => m.Lote)
                    .ThenInclude(l => l!.Medicamento)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new
                {
                    m.IdMovimiento,
                    m.IdLote,
                    Lote = m.Lote != null ? m.Lote.NumeroLote : null,
                    Medicamento = m.Lote != null && m.Lote.Medicamento != null
                        ? m.Lote.Medicamento.Nombre
                        : null,
                    m.IdUsuario,
                    Usuario = m.Usuario != null
                        ? m.Usuario.UsuarioLogin
                        : null,
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.FechaMovimiento,
                    m.Descripcion
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "MovimientosInventarioConsultar")]
        public async Task<IActionResult> GetMovimiento(int id)
        {
            var movimiento = await _context.MovimientosInventario
                .Include(m => m.Lote)
                    .ThenInclude(l => l!.Medicamento)
                .Include(m => m.Usuario)
                .Where(m => m.IdMovimiento == id)
                .Select(m => new
                {
                    m.IdMovimiento,
                    m.IdLote,
                    Lote = m.Lote != null ? m.Lote.NumeroLote : null,
                    Medicamento = m.Lote != null && m.Lote.Medicamento != null
                        ? m.Lote.Medicamento.Nombre
                        : null,
                    m.IdUsuario,
                    Usuario = m.Usuario != null
                        ? m.Usuario.UsuarioLogin
                        : null,
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.FechaMovimiento,
                    m.Descripcion
                })
                .FirstOrDefaultAsync();

            if (movimiento == null)
            {
                return NotFound();
            }

            return Ok(movimiento);
        }

        [HttpPost]
        [Authorize(Policy = "MovimientosInventarioCrear")]
        public async Task<IActionResult> CrearMovimiento(
            MovimientoInventario movimiento)
        {
            var lote = await _context.LotesMedicamento
                .FindAsync(movimiento.IdLote);

            if (lote == null)
            {
                return BadRequest(new
                {
                    mensaje = "El lote indicado no existe."
                });
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == movimiento.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe."
                });
            }

            if (movimiento.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            var tipo = movimiento.TipoMovimiento.Trim().ToLower();

            if (tipo == "entrada")
            {
                lote.CantidadDisponible += movimiento.Cantidad;
            }
            else if (tipo == "salida")
            {
                if (lote.CantidadDisponible < movimiento.Cantidad)
                {
                    return BadRequest(new
                    {
                        mensaje = "No hay suficiente existencia en el lote."
                    });
                }

                lote.CantidadDisponible -= movimiento.Cantidad;
            }
            else
            {
                return BadRequest(new
                {
                    mensaje = "El tipo de movimiento debe ser Entrada o Salida."
                });
            }

            if (movimiento.FechaMovimiento == default)
            {
                movimiento.FechaMovimiento = DateTime.Now;
            }

            movimiento.TipoMovimiento =
                tipo == "entrada" ? "Entrada" : "Salida";

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMovimiento),
                new { id = movimiento.IdMovimiento },
                new
                {
                    movimiento.IdMovimiento,
                    movimiento.IdLote,
                    movimiento.IdUsuario,
                    movimiento.TipoMovimiento,
                    movimiento.Cantidad,
                    movimiento.FechaMovimiento,
                    movimiento.Descripcion,
                    CantidadDisponible = lote.CantidadDisponible
                });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "MovimientosInventarioEliminar")]
        public async Task<IActionResult> EliminarMovimiento(int id)
        {
            var movimiento = await _context.MovimientosInventario
                .FindAsync(id);

            if (movimiento == null)
            {
                return NotFound();
            }

            _context.MovimientosInventario.Remove(movimiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}