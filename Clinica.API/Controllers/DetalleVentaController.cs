using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleVentaController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public DetalleVentaController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "DetalleVentaConsultar")]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await _context.Set<DetalleVenta>()
                .Include(d => d.Lote)
                    .ThenInclude(l => l!.Medicamento)
                .OrderByDescending(d => d.IdDetalle)
                .Select(d => new
                {
                    d.IdDetalle,
                    d.IdVenta,
                    d.IdLote,
                    Lote = d.Lote != null
                        ? d.Lote.NumeroLote
                        : null,
                    Medicamento = d.Lote != null &&
                                  d.Lote.Medicamento != null
                        ? d.Lote.Medicamento.Nombre
                        : null,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "DetalleVentaConsultar")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var detalle = await _context.Set<DetalleVenta>()
                .Include(d => d.Lote)
                    .ThenInclude(l => l!.Medicamento)
                .Where(d => d.IdDetalle == id)
                .Select(d => new
                {
                    d.IdDetalle,
                    d.IdVenta,
                    d.IdLote,
                    Lote = d.Lote != null
                        ? d.Lote.NumeroLote
                        : null,
                    Medicamento = d.Lote != null &&
                                  d.Lote.Medicamento != null
                        ? d.Lote.Medicamento.Nombre
                        : null,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .FirstOrDefaultAsync();

            if (detalle == null)
            {
                return NotFound();
            }

            return Ok(detalle);
        }

        [HttpPost]
        [Authorize(Policy = "DetalleVentaCrear")]
        public async Task<IActionResult> CrearDetalle(DetalleVenta detalle)
        {
            var venta = await _context.Set<VentaFarmacia>()
                .FindAsync(detalle.IdVenta);

            if (venta == null)
            {
                return BadRequest(new
                {
                    mensaje = "La venta indicada no existe."
                });
            }

            var lote = await _context.Set<LoteMedicamento>()
                .Include(l => l.Medicamento)
                .FirstOrDefaultAsync(l => l.IdLote == detalle.IdLote);

            if (lote == null)
            {
                return BadRequest(new
                {
                    mensaje = "El lote indicado no existe."
                });
            }

            if (lote.Medicamento == null)
            {
                return BadRequest(new
                {
                    mensaje = "El lote no tiene un medicamento asociado."
                });
            }

            if (detalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            if (lote.CantidadDisponible < detalle.Cantidad)
            {
                return BadRequest(new
                {
                    mensaje = "No hay suficiente existencia en el lote."
                });
            }

            detalle.PrecioUnitario = lote.Medicamento.PrecioVenta;
            detalle.Total = detalle.Cantidad * detalle.PrecioUnitario;

            lote.CantidadDisponible -= detalle.Cantidad;
            venta.Total += detalle.Total;

            _context.Set<DetalleVenta>().Add(detalle);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDetalle),
                new { id = detalle.IdDetalle },
                new
                {
                    detalle.IdDetalle,
                    detalle.IdVenta,
                    detalle.IdLote,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.Total,
                    CantidadDisponible = lote.CantidadDisponible,
                    TotalVenta = venta.Total
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "DetalleVentaModificar")]
        public async Task<IActionResult> ModificarDetalle(
            int id,
            DetalleVenta detalle)
        {
            if (id != detalle.IdDetalle)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del detalle."
                });
            }

            var detalleExistente = await _context.Set<DetalleVenta>()
                .FirstOrDefaultAsync(d => d.IdDetalle == id);

            if (detalleExistente == null)
            {
                return NotFound();
            }

            if (detalle.IdVenta != detalleExistente.IdVenta)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede cambiar la venta asociada al detalle."
                });
            }

            if (detalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            var venta = await _context.Set<VentaFarmacia>()
                .FindAsync(detalleExistente.IdVenta);

            if (venta == null)
            {
                return BadRequest(new
                {
                    mensaje = "La venta asociada no existe."
                });
            }

            var loteAnterior = await _context.Set<LoteMedicamento>()
                .FindAsync(detalleExistente.IdLote);

            if (loteAnterior == null)
            {
                return BadRequest(new
                {
                    mensaje = "El lote anterior ya no existe."
                });
            }

            var loteNuevo = await _context.Set<LoteMedicamento>()
                .Include(l => l.Medicamento)
                .FirstOrDefaultAsync(l => l.IdLote == detalle.IdLote);

            if (loteNuevo == null)
            {
                return BadRequest(new
                {
                    mensaje = "El lote indicado no existe."
                });
            }

            if (loteNuevo.Medicamento == null)
            {
                return BadRequest(new
                {
                    mensaje = "El lote no tiene un medicamento asociado."
                });
            }

            int cantidadDisponibleReal = loteNuevo.CantidadDisponible;

            if (detalleExistente.IdLote == detalle.IdLote)
            {
                cantidadDisponibleReal += detalleExistente.Cantidad;
            }

            if (cantidadDisponibleReal < detalle.Cantidad)
            {
                return BadRequest(new
                {
                    mensaje = "No hay suficiente existencia en el lote."
                });
            }

            decimal totalAnterior = detalleExistente.Total;

            if (detalleExistente.IdLote == detalle.IdLote)
            {
                loteNuevo.CantidadDisponible += detalleExistente.Cantidad;
                loteNuevo.CantidadDisponible -= detalle.Cantidad;
            }
            else
            {
                loteAnterior.CantidadDisponible += detalleExistente.Cantidad;
                loteNuevo.CantidadDisponible -= detalle.Cantidad;
            }

            decimal precioUnitario = loteNuevo.Medicamento.PrecioVenta;
            decimal nuevoTotal = detalle.Cantidad * precioUnitario;

            venta.Total = venta.Total - totalAnterior + nuevoTotal;

            if (venta.Total < 0)
            {
                venta.Total = 0;
            }

            detalleExistente.IdLote = detalle.IdLote;
            detalleExistente.Cantidad = detalle.Cantidad;
            detalleExistente.PrecioUnitario = precioUnitario;
            detalleExistente.Total = nuevoTotal;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                detalleExistente.IdDetalle,
                detalleExistente.IdVenta,
                detalleExistente.IdLote,
                detalleExistente.Cantidad,
                detalleExistente.PrecioUnitario,
                detalleExistente.Total,
                CantidadDisponible = loteNuevo.CantidadDisponible,
                TotalVenta = venta.Total
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DetalleVentaEliminar")]
        public async Task<IActionResult> EliminarDetalle(int id)
        {
            var detalle = await _context.Set<DetalleVenta>()
                .FirstOrDefaultAsync(d => d.IdDetalle == id);

            if (detalle == null)
            {
                return NotFound();
            }

            var lote = await _context.Set<LoteMedicamento>()
                .FindAsync(detalle.IdLote);

            var venta = await _context.Set<VentaFarmacia>()
                .FindAsync(detalle.IdVenta);

            if (lote != null)
            {
                lote.CantidadDisponible += detalle.Cantidad;
            }

            if (venta != null)
            {
                venta.Total -= detalle.Total;

                if (venta.Total < 0)
                {
                    venta.Total = 0;
                }
            }

            _context.Set<DetalleVenta>().Remove(detalle);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}