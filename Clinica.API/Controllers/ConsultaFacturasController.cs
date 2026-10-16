using Clinica.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultaFacturasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public ConsultaFacturasController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // RESUMEN DE FACTURAS
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetFacturas()
        {
            var facturas = await _context.Facturas
                .AsNoTracking()
                .OrderByDescending(f => f.FechaFactura)
                .ThenByDescending(f => f.IdFactura)
                .Select(f => new
                {
                    f.IdFactura,
                    f.NumeroFactura,

                    f.IdPaciente,

                    Paciente = f.Paciente != null
                        ? f.Paciente.Nombres + " " +
                          f.Paciente.Apellidos
                        : "Consumidor final",

                    f.IdUsuario,

                    Usuario = f.Usuario != null
                        ? f.Usuario.UsuarioLogin
                        : null,

                    f.IdVenta,

                    Origen = f.IdVenta != null
                        ? "Farmacia"
                        : "Clínica",

                    f.FechaFactura,
                    f.Total,
                    f.Estado,

                    CantidadDetalles =
                        f.Detalles.Count,

                    TieneHospitalizacion =
                        f.Detalles.Any(d =>
                            d.IdAsignacionHabitacion != null)
                })
                .ToListAsync();

            return Ok(facturas);
        }

        // =========================================================
        // TODOS LOS DETALLES
        // =========================================================

        [HttpGet("detalles")]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await _context.DetallesFactura
                .AsNoTracking()
                .OrderByDescending(d =>
                    d.Factura!.FechaFactura)
                .ThenByDescending(d =>
                    d.IdDetalleFactura)
                .Select(d => new
                {
                    d.IdDetalleFactura,

                    d.IdFactura,

                    NumeroFactura =
                        d.Factura != null
                            ? d.Factura.NumeroFactura
                            : null,

                    FechaFactura =
                        d.Factura != null
                            ? d.Factura.FechaFactura
                            : default,

                    Estado =
                        d.Factura != null
                            ? d.Factura.Estado
                            : null,

                    IdPaciente =
                        d.Factura != null
                            ? d.Factura.IdPaciente
                            : null,

                    Paciente =
                        d.Factura != null &&
                        d.Factura.Paciente != null
                            ? d.Factura.Paciente.Nombres +
                              " " +
                              d.Factura.Paciente.Apellidos
                            : "Consumidor final",

                    IdUsuario =
                        d.Factura != null
                            ? d.Factura.IdUsuario
                            : 0,

                    Usuario =
                        d.Factura != null &&
                        d.Factura.Usuario != null
                            ? d.Factura.Usuario.UsuarioLogin
                            : null,

                    IdVenta =
                        d.Factura != null
                            ? d.Factura.IdVenta
                            : null,

                    Origen =
                        d.Factura != null &&
                        d.Factura.IdVenta != null
                            ? "Farmacia"
                            : "Clínica",

                    TotalFactura =
                        d.Factura != null
                            ? d.Factura.Total
                            : 0,

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

                    FechaIngreso =
                        d.AsignacionHabitacion != null
                            ? (DateTime?)
                                d.AsignacionHabitacion
                                    .FechaIngreso
                            : null,

                    FechaEgreso =
                        d.AsignacionHabitacion != null
                            ? d.AsignacionHabitacion
                                .FechaEgreso
                            : null,

                    Habitacion =
                        d.AsignacionHabitacion != null &&
                        d.AsignacionHabitacion.Habitacion != null
                            ? d.AsignacionHabitacion
                                .Habitacion.Numero
                            : null,

                    d.Cantidad,
                    d.PrecioUnitario,

                    Subtotal =
                        d.Total
                })
                .ToListAsync();

            return Ok(detalles);
        }

        // =========================================================
        // FACTURA ESPECIFICA CON SUS DETALLES
        // =========================================================

        [HttpGet("{idFactura}")]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetFactura(
            int idFactura)
        {
            var factura = await _context.Facturas
                .AsNoTracking()
                .Where(f =>
                    f.IdFactura == idFactura)
                .Select(f => new
                {
                    f.IdFactura,
                    f.NumeroFactura,

                    f.IdPaciente,

                    Paciente =
                        f.Paciente != null
                            ? f.Paciente.Nombres +
                              " " +
                              f.Paciente.Apellidos
                            : "Consumidor final",

                    f.IdUsuario,

                    Usuario =
                        f.Usuario != null
                            ? f.Usuario.UsuarioLogin
                            : null,

                    f.IdVenta,

                    Origen =
                        f.IdVenta != null
                            ? "Farmacia"
                            : "Clínica",

                    f.FechaFactura,
                    f.Total,
                    f.Estado,

                    TieneHospitalizacion =
                        f.Detalles.Any(d =>
                            d.IdAsignacionHabitacion != null),

                    Detalles =
                        f.Detalles
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
                                        ? d.Catalogo
                                            .IdMedicamento
                                        : null,

                                Medicamento =
                                    d.Catalogo != null &&
                                    d.Catalogo
                                        .Medicamento != null
                                        ? d.Catalogo
                                            .Medicamento.Nombre
                                        : null,

                                d.IdAsignacionHabitacion,

                                EsHospitalizacion =
                                    d.IdAsignacionHabitacion !=
                                    null,

                                FechaIngreso =
                                    d.AsignacionHabitacion != null
                                        ? (DateTime?)
                                            d.AsignacionHabitacion
                                                .FechaIngreso
                                        : null,

                                FechaEgreso =
                                    d.AsignacionHabitacion != null
                                        ? d.AsignacionHabitacion
                                            .FechaEgreso
                                        : null,

                                Habitacion =
                                    d.AsignacionHabitacion != null &&
                                    d.AsignacionHabitacion
                                        .Habitacion != null
                                        ? d.AsignacionHabitacion
                                            .Habitacion.Numero
                                        : null,

                                d.Cantidad,
                                d.PrecioUnitario,

                                Subtotal =
                                    d.Total
                            })
                })
                .FirstOrDefaultAsync();

            if (factura == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "La factura indicada no existe."
                });
            }

            return Ok(factura);
        }
    }
}