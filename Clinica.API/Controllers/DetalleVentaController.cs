using Clinica.API.Data;
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

        public DetalleVentaController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "DetalleVentaConsultar")]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles =
                await (
                    from detalle in _context.DetallesVenta

                    join venta in _context.VentasFarmacia
                        on detalle.IdVenta equals venta.IdVenta

                    join usuario in _context.Usuarios
                        on venta.IdUsuario equals usuario.IdUsuario

                    join lote in _context.LotesMedicamento
                        on detalle.IdLote equals lote.IdLote

                    join medicamento in _context.Medicamentos
                        on lote.IdMedicamento equals medicamento.IdMedicamento

                    join marca in _context.Marcas
                        on medicamento.IdMarca equals marca.IdMarca
                        into marcas

                    from marca in marcas.DefaultIfEmpty()

                    orderby venta.FechaVenta descending,
                            detalle.IdDetalle

                    select new
                    {
                        detalle.IdDetalle,

                        venta.IdVenta,
                        venta.FechaVenta,
                        venta.IdUsuario,

                        Usuario =
                            usuario.UsuarioLogin,

                        TotalVenta =
                            venta.Total,

                        detalle.IdLote,

                        Lote =
                            lote.NumeroLote,

                        lote.FechaIngreso,
                        lote.FechaVencimiento,

                        medicamento.IdMedicamento,

                        Medicamento =
                            medicamento.Nombre,

                        medicamento.IdMarca,

                        Marca =
                            marca != null
                                ? marca.Nombre
                                : null,

                        detalle.Cantidad,
                        detalle.PrecioUnitario,

                        Subtotal =
                            detalle.Total
                    }
                )
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "DetalleVentaConsultar")]
        public async Task<IActionResult> GetDetalle(
            int id)
        {
            var detalle =
                await (
                    from d in _context.DetallesVenta

                    join venta in _context.VentasFarmacia
                        on d.IdVenta equals venta.IdVenta

                    join usuario in _context.Usuarios
                        on venta.IdUsuario equals usuario.IdUsuario

                    join lote in _context.LotesMedicamento
                        on d.IdLote equals lote.IdLote

                    join medicamento in _context.Medicamentos
                        on lote.IdMedicamento equals medicamento.IdMedicamento

                    join marca in _context.Marcas
                        on medicamento.IdMarca equals marca.IdMarca
                        into marcas

                    from marca in marcas.DefaultIfEmpty()

                    where d.IdDetalle == id

                    select new
                    {
                        d.IdDetalle,

                        venta.IdVenta,
                        venta.FechaVenta,
                        venta.IdUsuario,

                        Usuario =
                            usuario.UsuarioLogin,

                        TotalVenta =
                            venta.Total,

                        d.IdLote,

                        Lote =
                            lote.NumeroLote,

                        lote.FechaIngreso,
                        lote.FechaVencimiento,

                        medicamento.IdMedicamento,

                        Medicamento =
                            medicamento.Nombre,

                        medicamento.IdMarca,

                        Marca =
                            marca != null
                                ? marca.Nombre
                                : null,

                        d.Cantidad,
                        d.PrecioUnitario,

                        Subtotal =
                            d.Total
                    }
                )
                .FirstOrDefaultAsync();

            if (detalle == null)
            {
                return NotFound();
            }

            return Ok(detalle);
        }

        [HttpGet("venta/{idVenta}")]
        [Authorize(Policy = "DetalleVentaConsultar")]
        public async Task<IActionResult> GetPorVenta(
            int idVenta)
        {
            var venta =
                await _context.VentasFarmacia
                    .Include(v => v.Usuario)
                    .Where(v =>
                        v.IdVenta == idVenta)
                    .Select(v => new
                    {
                        v.IdVenta,
                        v.FechaVenta,
                        v.IdUsuario,

                        Usuario =
                            v.Usuario != null
                                ? v.Usuario.UsuarioLogin
                                : null,

                        v.Total
                    })
                    .FirstOrDefaultAsync();

            if (venta == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "La venta indicada no existe."
                });
            }

            var detalles =
                await (
                    from detalle in _context.DetallesVenta

                    join lote in _context.LotesMedicamento
                        on detalle.IdLote equals lote.IdLote

                    join medicamento in _context.Medicamentos
                        on lote.IdMedicamento equals medicamento.IdMedicamento

                    join marca in _context.Marcas
                        on medicamento.IdMarca equals marca.IdMarca
                        into marcas

                    from marca in marcas.DefaultIfEmpty()

                    where detalle.IdVenta == idVenta

                    orderby detalle.IdDetalle

                    select new
                    {
                        detalle.IdDetalle,
                        detalle.IdLote,

                        Lote =
                            lote.NumeroLote,

                        lote.FechaIngreso,
                        lote.FechaVencimiento,

                        medicamento.IdMedicamento,

                        Medicamento =
                            medicamento.Nombre,

                        medicamento.IdMarca,

                        Marca =
                            marca != null
                                ? marca.Nombre
                                : null,

                        detalle.Cantidad,
                        detalle.PrecioUnitario,

                        Subtotal =
                            detalle.Total
                    }
                )
                .ToListAsync();

            return Ok(new
            {
                venta.IdVenta,
                venta.FechaVenta,
                venta.IdUsuario,
                venta.Usuario,
                venta.Total,
                CantidadUnidades =
                    detalles.Sum(d => d.Cantidad),
                CantidadLineas =
                    detalles.Count,
                Detalles =
                    detalles
            });
        }
    }
}