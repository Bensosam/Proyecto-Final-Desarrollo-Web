using Clinica.API.Data;
using Clinica.API.DTOs;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasFarmaciaController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public VentasFarmaciaController(
            ClinicaDbContext context)
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
                    v.Total,

                    CantidadProductos =
                        v.Detalles.Sum(d => d.Cantidad)
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("productos")]
        [Authorize(Policy = "VentasFarmaciaConsultar")]
        public async Task<IActionResult> GetProductosVenta()
        {
            var fechaActual =
                ObtenerFechaActualLocal();

            var productos = await _context.Medicamentos
                .Include(m => m.Marca)
                .Where(m => m.Estado)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.IdMedicamento,
                    m.Nombre,

                    Marca = m.Marca != null
                        ? m.Marca.Nombre
                        : null,

                    m.Descripcion,
                    m.PrecioVenta,
                    m.Imagen,

                    ExistenciaDisponible =
                        m.Lotes
                            .Where(l =>
                                l.FechaVencimiento >= fechaActual &&
                                l.CantidadDisponible > 0)
                            .Sum(l =>
                                (int?)l.CantidadDisponible) ?? 0,

                    Lotes = m.Lotes
                        .Where(l =>
                            l.FechaVencimiento >= fechaActual &&
                            l.CantidadDisponible > 0)
                        .OrderBy(l =>
                            l.FechaVencimiento)
                        .ThenBy(l =>
                            l.NumeroLote)
                        .Select(l => new
                        {
                            l.IdLote,
                            l.NumeroLote,
                            l.FechaIngreso,
                            l.FechaVencimiento,
                            l.CantidadDisponible
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(productos);
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
                    v.Total,

                    Detalles = v.Detalles
                        .OrderBy(d => d.IdDetalle)
                        .Select(d => new
                        {
                            d.IdDetalle,
                            d.IdLote,

                            Lote = d.Lote != null
                                ? d.Lote.NumeroLote
                                : null,

                            FechaVencimiento =
                                d.Lote != null
                                    ? d.Lote.FechaVencimiento
                                    : (DateTime?)null,

                            IdMedicamento =
                                d.Lote != null
                                    ? d.Lote.IdMedicamento
                                    : 0,

                            Medicamento =
                                d.Lote != null &&
                                d.Lote.Medicamento != null
                                    ? d.Lote.Medicamento.Nombre
                                    : null,

                            d.Cantidad,
                            d.PrecioUnitario,
                            d.Total
                        })
                        .ToList()
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
        public async Task<IActionResult> CrearVenta(
            GuardarVentaDto request)
        {
            var idUsuario =
                ObtenerIdUsuario();

            if (idUsuario == null)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "No fue posible identificar al usuario autenticado."
                });
            }

            var usuarioExiste =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.IdUsuario == idUsuario.Value &&
                        u.Estado);

            if (!usuarioExiste)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "El usuario autenticado no existe o está inactivo."
                });
            }

            var errorBasico =
                ValidarItemsBasicos(request.Items);

            if (errorBasico != null)
            {
                return BadRequest(new
                {
                    mensaje = errorBasico
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                var venta = new VentaFarmacia
                {
                    IdUsuario = idUsuario.Value,
                    FechaVenta = ObtenerFechaHoraLocal(),
                    Total = 0
                };

                _context.VentasFarmacia.Add(venta);

                await _context.SaveChangesAsync();

                var error =
                    await AgregarItemsAsync(
                        venta,
                        request.Items);

                if (error != null)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        mensaje = error
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "VentasFarmaciaModificar")]
        public async Task<IActionResult> ModificarVenta(
            int id,
            GuardarVentaDto request)
        {
            var errorBasico =
                ValidarItemsBasicos(request.Items);

            if (errorBasico != null)
            {
                return BadRequest(new
                {
                    mensaje = errorBasico
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                var venta = await _context.VentasFarmacia
                    .Include(v => v.Detalles)
                    .FirstOrDefaultAsync(
                        v => v.IdVenta == id);

                if (venta == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }
var ventaFacturada =
    await _context.Facturas
        .AnyAsync(f =>
            f.IdVenta == id);

if (ventaFacturada)
{
    return BadRequest(new
    {
        mensaje =
            "No se puede modificar una venta que ya fue facturada."
    });
}

                var idsLotesAnteriores =
                    venta.Detalles
                        .Select(d => d.IdLote)
                        .Distinct()
                        .ToList();

                var lotesAnteriores =
                    await _context.LotesMedicamento
                        .Where(l =>
                            idsLotesAnteriores.Contains(
                                l.IdLote))
                        .ToDictionaryAsync(
                            l => l.IdLote);

                foreach (var detalle in venta.Detalles)
                {
                    if (lotesAnteriores.TryGetValue(
                        detalle.IdLote,
                        out var lote))
                    {
                        lote.CantidadDisponible +=
                            detalle.Cantidad;
                    }
                }

                _context.Set<DetalleVenta>()
                    .RemoveRange(venta.Detalles);

                venta.Total = 0;

                var error =
                    await AgregarItemsAsync(
                        venta,
                        request.Items);

                if (error != null)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        mensaje = error
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    venta.IdVenta,
                    venta.IdUsuario,
                    venta.FechaVenta,
                    venta.Total
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "VentasFarmaciaEliminar")]
        public async Task<IActionResult> EliminarVenta(
            int id)
        {
            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                var venta = await _context.VentasFarmacia
                    .Include(v => v.Detalles)
                    .FirstOrDefaultAsync(
                        v => v.IdVenta == id);

                if (venta == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

var ventaFacturada =
    await _context.Facturas
        .AnyAsync(f =>
            f.IdVenta == id);

if (ventaFacturada)
{
    return BadRequest(new
    {
        mensaje =
            "No se puede eliminar una venta que ya fue facturada."
    });
}


                var idsLotes =
                    venta.Detalles
                        .Select(d => d.IdLote)
                        .Distinct()
                        .ToList();

                var lotes =
                    await _context.LotesMedicamento
                        .Where(l =>
                            idsLotes.Contains(l.IdLote))
                        .ToDictionaryAsync(
                            l => l.IdLote);

                foreach (var detalle in venta.Detalles)
                {
                    if (lotes.TryGetValue(
                        detalle.IdLote,
                        out var lote))
                    {
                        lote.CantidadDisponible +=
                            detalle.Cantidad;
                    }
                }

                _context.VentasFarmacia.Remove(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string?> AgregarItemsAsync(
            VentaFarmacia venta,
            List<ItemVentaDto> items)
        {
            var itemsAgrupados = items
                .GroupBy(i => i.IdLote)
                .Select(g => new ItemVentaDto
                {
                    IdLote = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .ToList();

            var idsLotes =
                itemsAgrupados
                    .Select(i => i.IdLote)
                    .ToList();

            var lotes =
                await _context.LotesMedicamento
                    .Include(l => l.Medicamento)
                    .Where(l =>
                        idsLotes.Contains(l.IdLote))
                    .ToDictionaryAsync(
                        l => l.IdLote);

            var fechaActual =
                ObtenerFechaActualLocal();

            foreach (var item in itemsAgrupados)
            {
                if (!lotes.TryGetValue(
                    item.IdLote,
                    out var lote))
                {
                    return
                        $"El lote con ID {item.IdLote} no existe.";
                }

                if (lote.Medicamento == null)
                {
                    return
                        $"El lote {lote.NumeroLote} no tiene un medicamento asociado.";
                }

                if (!lote.Medicamento.Estado)
                {
                    return
                        $"El medicamento {lote.Medicamento.Nombre} está inactivo.";
                }

                if (lote.FechaVencimiento.Date <
                    fechaActual)
                {
                    return
                        $"El lote {lote.NumeroLote} de {lote.Medicamento.Nombre} está vencido.";
                }

                if (lote.CantidadDisponible <
                    item.Cantidad)
                {
                    return
                        $"No hay suficiente existencia en el lote {lote.NumeroLote} " +
                        $"de {lote.Medicamento.Nombre}. " +
                        $"Disponible: {lote.CantidadDisponible}. " +
                        $"Solicitado: {item.Cantidad}.";
                }

                var detalle =
                    new DetalleVenta
                    {
                        IdVenta =
                            venta.IdVenta,

                        IdLote =
                            lote.IdLote,

                        Cantidad =
                            item.Cantidad,

                        PrecioUnitario =
                            lote.Medicamento.PrecioVenta,

                        Total =
                            item.Cantidad *
                            lote.Medicamento.PrecioVenta
                    };

                lote.CantidadDisponible -=
                    item.Cantidad;

                venta.Total +=
                    detalle.Total;

                _context.Set<DetalleVenta>()
                    .Add(detalle);
            }

            return null;
        }

        private int? ObtenerIdUsuario()
        {
            var claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                claim,
                out var idUsuario))
            {
                return null;
            }

            return idUsuario;
        }

        private static string? ValidarItemsBasicos(
            List<ItemVentaDto>? items)
        {
            if (items == null ||
                items.Count == 0)
            {
                return
                    "La venta debe contener al menos un medicamento.";
            }

            if (items.Any(i =>
                i.IdLote <= 0))
            {
                return
                    "Todos los lotes de la venta deben ser válidos.";
            }

            if (items.Any(i =>
                i.Cantidad <= 0))
            {
                return
                    "Todas las cantidades deben ser mayores que cero.";
            }

            return null;
        }

        private static DateTime
            ObtenerFechaActualLocal()
        {
            return ObtenerFechaHoraLocal().Date;
        }

        private static DateTime
            ObtenerFechaHoraLocal()
        {
            var utcNow = DateTime.UtcNow;

            try
            {
                var zona =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        "America/Guatemala");

                return TimeZoneInfo.ConvertTimeFromUtc(
                    utcNow,
                    zona);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var zona =
                        TimeZoneInfo.FindSystemTimeZoneById(
                            "Central America Standard Time");

                    return TimeZoneInfo.ConvertTimeFromUtc(
                        utcNow,
                        zona);
                }
                catch (TimeZoneNotFoundException)
                {
                    return DateTime.Now;
                }
            }
        }
    }
}