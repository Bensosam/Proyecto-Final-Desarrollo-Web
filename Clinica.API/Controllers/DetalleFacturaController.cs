using Clinica.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleFacturaController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public DetalleFacturaController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CONSULTAR TODOS LOS DETALLES
        // SOLO LECTURA
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await _context.DetallesFactura
                .AsNoTracking()
                .OrderByDescending(d => d.IdDetalleFactura)
                .Select(d => new
                {
                    d.IdDetalleFactura,
                    d.IdFactura,

                    NumeroFactura =
                        d.Factura != null
                            ? d.Factura.NumeroFactura
                            : null,

                    d.IdCatalogo,

                    Codigo =
                        d.Catalogo != null
                            ? d.Catalogo.Codigo
                            : null,

                    Tipo =
                        d.Catalogo != null
                            ? d.Catalogo.Tipo
                            : null,

                    Catalogo =
                        d.Catalogo != null
                            ? d.Catalogo.Nombre
                            : null,

                    d.Descripcion,

                    IdMedicamento =
                        d.Catalogo != null
                            ? d.Catalogo.IdMedicamento
                            : null,

                    Medicamento =
                        d.Catalogo != null &&
                        d.Catalogo.Medicamento != null
                            ? d.Catalogo.Medicamento.Nombre
                            : null,

                    d.IdAsignacionHabitacion,

                    EsHospitalizacion =
                        d.IdAsignacionHabitacion != null,

                    Habitacion =
                        d.AsignacionHabitacion != null &&
                        d.AsignacionHabitacion.Habitacion != null
                            ? d.AsignacionHabitacion.Habitacion.Numero
                            : null,

                    FechaIngreso =
                        d.AsignacionHabitacion != null
                            ? (DateTime?)d.AsignacionHabitacion.FechaIngreso
                            : null,

                    FechaEgreso =
                        d.AsignacionHabitacion != null
                            ? d.AsignacionHabitacion.FechaEgreso
                            : null,

                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .ToListAsync();

            return Ok(detalles);
        }

        // =========================================================
        // CONSULTAR UN DETALLE
        // =========================================================

        [HttpGet("{id}")]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetDetalle(
            int id)
        {
            var detalle = await _context.DetallesFactura
                .AsNoTracking()
                .Where(d =>
                    d.IdDetalleFactura == id)
                .Select(d => new
                {
                    d.IdDetalleFactura,
                    d.IdFactura,

                    NumeroFactura =
                        d.Factura != null
                            ? d.Factura.NumeroFactura
                            : null,

                    d.IdCatalogo,

                    Codigo =
                        d.Catalogo != null
                            ? d.Catalogo.Codigo
                            : null,

                    Tipo =
                        d.Catalogo != null
                            ? d.Catalogo.Tipo
                            : null,

                    Catalogo =
                        d.Catalogo != null
                            ? d.Catalogo.Nombre
                            : null,

                    d.Descripcion,

                    IdMedicamento =
                        d.Catalogo != null
                            ? d.Catalogo.IdMedicamento
                            : null,

                    Medicamento =
                        d.Catalogo != null &&
                        d.Catalogo.Medicamento != null
                            ? d.Catalogo.Medicamento.Nombre
                            : null,

                    d.IdAsignacionHabitacion,

                    EsHospitalizacion =
                        d.IdAsignacionHabitacion != null,

                    Habitacion =
                        d.AsignacionHabitacion != null &&
                        d.AsignacionHabitacion.Habitacion != null
                            ? d.AsignacionHabitacion.Habitacion.Numero
                            : null,

                    FechaIngreso =
                        d.AsignacionHabitacion != null
                            ? (DateTime?)d.AsignacionHabitacion.FechaIngreso
                            : null,

                    FechaEgreso =
                        d.AsignacionHabitacion != null
                            ? d.AsignacionHabitacion.FechaEgreso
                            : null,

                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .FirstOrDefaultAsync();

            if (detalle == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El detalle de factura indicado no existe."
                });
            }

            return Ok(detalle);
        }

        // =========================================================
        // CONSULTAR DETALLES DE UNA FACTURA
        // =========================================================

        [HttpGet("factura/{idFactura}")]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetPorFactura(
            int idFactura)
        {
            var facturaExiste =
                await _context.Facturas
                    .AsNoTracking()
                    .AnyAsync(f =>
                        f.IdFactura == idFactura);

            if (!facturaExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        "La factura indicada no existe."
                });
            }

            var detalles = await _context.DetallesFactura
                .AsNoTracking()
                .Where(d =>
                    d.IdFactura == idFactura)
                .OrderBy(d =>
                    d.IdDetalleFactura)
                .Select(d => new
                {
                    d.IdDetalleFactura,
                    d.IdFactura,
                    d.IdCatalogo,

                    Codigo =
                        d.Catalogo != null
                            ? d.Catalogo.Codigo
                            : null,

                    Tipo =
                        d.Catalogo != null
                            ? d.Catalogo.Tipo
                            : null,

                    Catalogo =
                        d.Catalogo != null
                            ? d.Catalogo.Nombre
                            : null,

                    d.Descripcion,

                    IdMedicamento =
                        d.Catalogo != null
                            ? d.Catalogo.IdMedicamento
                            : null,

                    Medicamento =
                        d.Catalogo != null &&
                        d.Catalogo.Medicamento != null
                            ? d.Catalogo.Medicamento.Nombre
                            : null,

                    d.IdAsignacionHabitacion,

                    EsHospitalizacion =
                        d.IdAsignacionHabitacion != null,

                    Habitacion =
                        d.AsignacionHabitacion != null &&
                        d.AsignacionHabitacion.Habitacion != null
                            ? d.AsignacionHabitacion.Habitacion.Numero
                            : null,

                    FechaIngreso =
                        d.AsignacionHabitacion != null
                            ? (DateTime?)d.AsignacionHabitacion.FechaIngreso
                            : null,

                    FechaEgreso =
                        d.AsignacionHabitacion != null
                            ? d.AsignacionHabitacion.FechaEgreso
                            : null,

                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .ToListAsync();

            return Ok(detalles);
        }
    }
}