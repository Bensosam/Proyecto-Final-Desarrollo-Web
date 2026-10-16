using System.Data;
using System.Security.Claims;
using Clinica.API.Data;
using Clinica.API.DTOs;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public FacturasController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CONSULTAR FACTURAS
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult> GetFacturas()
        {
            var datos = await _context.Facturas
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
                    f.FechaFactura,
                    f.Total,
                    f.Estado,

                    CantidadConceptos =
                        f.Detalles.Count,

                    CantidadDetallesVenta =
                        f.Venta != null
                            ? f.Venta.Detalles.Count
                            : 0
                })
                .ToListAsync();

            var facturas = datos.Select(f => new
            {
                f.IdFactura,
                f.NumeroFactura,
                f.IdPaciente,
                f.Paciente,
                f.IdUsuario,
                f.Usuario,
                f.IdVenta,

                Origen =
                    f.IdVenta == null
                        ? "Clínica"
                        : f.CantidadConceptos >
                          f.CantidadDetallesVenta
                            ? "Mixta"
                            : "Farmacia",

                f.FechaFactura,
                f.Total,
                f.Estado,
                f.CantidadConceptos
            });

            return Ok(facturas);
        }

        // =========================================================
        // CONSULTAR UNA FACTURA
        // =========================================================

        [HttpGet("{id}")]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult> GetFactura(
            int id)
        {
            var factura = await _context.Facturas
                .AsNoTracking()
                .Where(f =>
                    f.IdFactura == id)
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
                    f.FechaFactura,
                    f.Total,
                    f.Estado,

                    CantidadDetallesVenta =
                        f.Venta != null
                            ? f.Venta.Detalles.Count
                            : 0,

                    Detalles = f.Detalles
                        .OrderBy(d =>
                            d.IdDetalleFactura)
                        .Select(d => new
                        {
                            d.IdDetalleFactura,
                            d.IdCatalogo,

                            Codigo =
                                d.Catalogo != null
                                    ? d.Catalogo.Codigo
                                    : null,

                            Catalogo =
                                d.Catalogo != null
                                    ? d.Catalogo.Nombre
                                    : null,

                            Tipo =
                                d.Catalogo != null
                                    ? d.Catalogo.Tipo
                                    : null,

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
                            d.Descripcion,
                            d.Cantidad,
                            d.PrecioUnitario,

                            Subtotal =
                                d.Total
                        })
                        .ToList()
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

            var origen =
                factura.IdVenta == null
                    ? "Clínica"
                    : factura.Detalles.Count >
                      factura.CantidadDetallesVenta
                        ? "Mixta"
                        : "Farmacia";

            return Ok(new
            {
                factura.IdFactura,
                factura.NumeroFactura,
                factura.IdPaciente,
                factura.Paciente,
                factura.IdUsuario,
                factura.Usuario,
                factura.IdVenta,
                Origen = origen,
                factura.FechaFactura,
                factura.Total,
                factura.Estado,
                factura.Detalles
            });
        }

        // =========================================================
        // VENTAS DE FARMACIA TODAVIA NO FACTURADAS
        // =========================================================

        [HttpGet("ventas-disponibles")]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult>
            GetVentasDisponibles()
        {
            var ventas = await _context.VentasFarmacia
                .AsNoTracking()
                .Where(v =>
                    v.Detalles.Any() &&
                    !_context.Facturas.Any(f =>
                        f.IdVenta == v.IdVenta))
                .OrderByDescending(v =>
                    v.FechaVenta)
                .ThenByDescending(v =>
                    v.IdVenta)
                .Select(v => new
                {
                    v.IdVenta,
                    v.FechaVenta,

                    Usuario =
                        v.Usuario != null
                            ? v.Usuario.UsuarioLogin
                            : null,

                    v.Total,

                    CantidadUnidades =
                        v.Detalles.Sum(d =>
                            d.Cantidad),

                    CantidadLineas =
                        v.Detalles.Count,

                    Detalles =
                        v.Detalles
                            .OrderBy(d =>
                                d.IdDetalle)
                            .Select(d => new
                            {
                                d.IdDetalle,
                                d.IdLote,

                                Lote =
                                    d.Lote != null
                                        ? d.Lote.NumeroLote
                                        : null,

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

                                Subtotal =
                                    d.Total
                            })
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // =========================================================
        // CATALOGO PARA FACTURACION
        // =========================================================

        [HttpGet("catalogo-disponible")]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult>
            GetCatalogoDisponible()
        {
            var catalogo = await _context.Catalogos
                .AsNoTracking()
                .Where(c => c.Estado)
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Nombre)
                .Select(c => new
                {
                    c.IdCatalogo,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.Tipo,
                    c.Precio,
                    c.IdMedicamento,

                    Medicamento =
                        c.Medicamento != null
                            ? c.Medicamento.Nombre
                            : null,

                    c.IdTipoHabitacion,

                    TipoHabitacion =
                        c.TipoHabitacion != null
                            ? c.TipoHabitacion.Nombre
                            : null,

                    EsHospitalizacion =
                        c.IdTipoHabitacion != null
                })
                .ToListAsync();

            return Ok(catalogo);
        }

        // =========================================================
        // HOSPITALIZACIONES PENDIENTES DE FACTURAR
        // =========================================================

        [HttpGet(
            "hospitalizaciones-disponibles/{idPaciente}")]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult>
            GetHospitalizacionesDisponibles(
                int idPaciente)
        {
            var pacienteExiste =
                await _context.Pacientes
                    .AnyAsync(p =>
                        p.IdPaciente == idPaciente);

            if (!pacienteExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        "El paciente indicado no existe."
                });
            }

            var asignaciones =
                await _context.AsignacionesHabitacion
                    .AsNoTracking()
                    .Include(a => a.Habitacion)
                        .ThenInclude(h =>
                            h!.TipoHabitacion)
                    .Where(a =>
                        a.IdPaciente == idPaciente &&
                        a.FechaEgreso != null &&
                        a.FechaEgreso >
                            a.FechaIngreso &&
                        !_context.DetallesFactura.Any(d =>
                            d.IdAsignacionHabitacion ==
                            a.IdAsignacion))
                    .OrderByDescending(a =>
                        a.FechaIngreso)
                    .ToListAsync();

            var resultado =
                asignaciones.Select(a =>
                {
                    var dias =
                        Math.Max(
                            1,
                            (int)Math.Ceiling(
                                (a.FechaEgreso!.Value -
                                 a.FechaIngreso)
                                .TotalDays));

                    var precioPorDia =
                        a.Habitacion
                            ?.TipoHabitacion
                            ?.PrecioPorDia ?? 0;

                    return new
                    {
                        a.IdAsignacion,
                        a.IdPaciente,
                        a.IdHabitacion,

                        Habitacion =
                            a.Habitacion?.Numero,

                        IdTipoHabitacion =
                            a.Habitacion
                                ?.IdTipoHabitacion,

                        TipoHabitacion =
                            a.Habitacion
                                ?.TipoHabitacion
                                ?.Nombre,

                        a.FechaIngreso,
                        a.FechaEgreso,
                        Dias = dias,
                        PrecioPorDia = precioPorDia,

                        Total =
                            dias * precioPorDia
                    };
                })
                .ToList();

            return Ok(resultado);
        }

        // =========================================================
        // CREAR FACTURA
        //
        // PUEDE CONTENER:
        // - Venta de farmacia
        // - Productos / servicios clinicos
        // - Hospitalizacion
        // - Cualquier combinacion de los anteriores
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "FacturasCrear")]
        public async Task<IActionResult> CrearFactura(
            GuardarFacturaDto request)
        {
            var idUsuarioTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    idUsuarioTexto,
                    out var idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje =
                        "No fue posible identificar al usuario."
                });
            }

            var usuarioExiste =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.IdUsuario == idUsuario &&
                        u.Estado);

            if (!usuarioExiste)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "El usuario de la sesión no existe o está inactivo."
                });
            }

            var itemsSolicitados =
                request.Items ?? [];

            var asignacionesSolicitadas =
                request.IdsAsignacionesHabitacion ?? [];

            var tieneVenta =
                request.IdVenta.HasValue &&
                request.IdVenta.Value > 0;

            var tieneItemsClinicos =
                itemsSolicitados.Any();

            var idsAsignaciones =
                asignacionesSolicitadas
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

            var tieneHospitalizacion =
                idsAsignaciones.Count > 0;

            var tieneContenidoClinico =
                tieneItemsClinicos ||
                tieneHospitalizacion;

            // Tiene que existir algo que facturar.
            if (!tieneVenta &&
                !tieneContenidoClinico)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La factura debe incluir una venta de farmacia, un producto, un servicio o una hospitalización."
                });
            }

            // Si incluye contenido clínico, el paciente es obligatorio.
            if (tieneContenidoClinico &&
                (!request.IdPaciente.HasValue ||
                 request.IdPaciente.Value <= 0))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Las facturas que incluyen servicios clínicos u hospitalización requieren un paciente."
                });
            }

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                // =================================================
                // VALIDAR PACIENTE
                // =================================================

                if (request.IdPaciente.HasValue)
                {
                    var pacienteExiste =
                        await _context.Pacientes
                            .AnyAsync(p =>
                                p.IdPaciente ==
                                request.IdPaciente.Value);

                    if (!pacienteExiste)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "El paciente indicado no existe."
                        });
                    }
                }

                var detallesFactura =
                    new List<DetalleFactura>();

                int? idVenta = null;

                // =================================================
                // AGREGAR VENTA DE FARMACIA
                // =================================================

                if (tieneVenta)
                {
                    idVenta =
                        request.IdVenta!.Value;

                    var ventaYaFacturada =
                        await _context.Facturas
                            .AnyAsync(f =>
                                f.IdVenta == idVenta);

                    if (ventaYaFacturada)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "La venta seleccionada ya tiene una factura."
                        });
                    }

                    var venta =
                        await _context.VentasFarmacia
                            .Include(v => v.Detalles)
                                .ThenInclude(d => d.Lote)
                                    .ThenInclude(l =>
                                        l!.Medicamento)
                            .FirstOrDefaultAsync(v =>
                                v.IdVenta == idVenta);

                    if (venta == null)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "La venta seleccionada no existe."
                        });
                    }

                    if (venta.Detalles.Count == 0)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "La venta seleccionada no contiene productos."
                        });
                    }

                    var idsMedicamentos =
                        venta.Detalles
                            .Where(d =>
                                d.Lote?.Medicamento != null)
                            .Select(d =>
                                d.Lote!
                                    .Medicamento!
                                    .IdMedicamento)
                            .Distinct()
                            .ToList();

                    var catalogosMedicamento =
                        await _context.Catalogos
                            .Where(c =>
                                c.IdMedicamento != null &&
                                idsMedicamentos.Contains(
                                    c.IdMedicamento.Value))
                            .OrderByDescending(c =>
                                c.Estado)
                            .ThenBy(c =>
                                c.IdCatalogo)
                            .ToListAsync();

                    foreach (var detalleVenta
                        in venta.Detalles)
                    {
                        var lote =
                            detalleVenta.Lote;

                        var medicamento =
                            lote?.Medicamento;

                        if (lote == null ||
                            medicamento == null)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    "Uno de los productos de la venta no tiene lote o medicamento asociado."
                            });
                        }

                        var catalogo =
                            catalogosMedicamento
                                .FirstOrDefault(c =>
                                    c.IdMedicamento ==
                                    medicamento.IdMedicamento);

                        if (catalogo == null)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    $"El medicamento '{medicamento.Nombre}' no tiene un producto vinculado en el catálogo de facturación."
                            });
                        }

                        detallesFactura.Add(
                            new DetalleFactura
                            {
                                IdCatalogo =
                                    catalogo.IdCatalogo,

                                Descripcion =
                                    $"{medicamento.Nombre} - Lote {lote.NumeroLote}",

                                Cantidad =
                                    detalleVenta.Cantidad,

                                PrecioUnitario =
                                    detalleVenta.PrecioUnitario,

                                Total =
                                    detalleVenta.Total
                            });
                    }
                }

                // =================================================
                // AGREGAR PRODUCTOS Y SERVICIOS CLINICOS
                // =================================================

                if (tieneItemsClinicos)
                {
                    if (itemsSolicitados.Any(i =>
                        i.IdCatalogo <= 0))
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "Uno o más conceptos de la factura son inválidos."
                        });
                    }

                    if (itemsSolicitados.Any(i =>
                        i.Cantidad <= 0))
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "Todas las cantidades deben ser mayores que cero."
                        });
                    }

                    var itemsAgrupados =
                        itemsSolicitados
                            .GroupBy(i =>
                                i.IdCatalogo)
                            .Select(g => new
                            {
                                IdCatalogo =
                                    g.Key,

                                Cantidad =
                                    g.Sum(x =>
                                        x.Cantidad)
                            })
                            .ToList();

                    var idsCatalogo =
                        itemsAgrupados
                            .Select(i =>
                                i.IdCatalogo)
                            .Distinct()
                            .ToList();

                    var catalogos =
                        await _context.Catalogos
                            .Where(c =>
                                idsCatalogo.Contains(
                                    c.IdCatalogo) &&
                                c.Estado)
                            .ToListAsync();

                    if (catalogos.Count !=
                        idsCatalogo.Count)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "Uno o más conceptos del catálogo no existen o están inactivos."
                        });
                    }

                    foreach (var item
                        in itemsAgrupados)
                    {
                        var catalogo =
                            catalogos.First(c =>
                                c.IdCatalogo ==
                                item.IdCatalogo);

                        if (catalogo
                            .IdTipoHabitacion != null)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    $"El concepto '{catalogo.Nombre}' corresponde a hospitalización y debe agregarse mediante una hospitalización finalizada."
                            });
                        }

                        detallesFactura.Add(
                            new DetalleFactura
                            {
                                IdCatalogo =
                                    catalogo.IdCatalogo,

                                Descripcion =
                                    catalogo.Nombre,

                                Cantidad =
                                    item.Cantidad,

                                PrecioUnitario =
                                    catalogo.Precio,

                                Total =
                                    item.Cantidad *
                                    catalogo.Precio
                            });
                    }
                }

                // =================================================
                // AGREGAR HOSPITALIZACIONES
                // =================================================

                if (tieneHospitalizacion)
                {
                    var idPaciente =
                        request.IdPaciente!.Value;

                    var asignaciones =
                        await _context
                            .AsignacionesHabitacion
                            .Include(a =>
                                a.Habitacion)
                                .ThenInclude(h =>
                                    h!.TipoHabitacion)
                            .Where(a =>
                                idsAsignaciones
                                    .Contains(
                                        a.IdAsignacion))
                            .ToListAsync();

                    if (asignaciones.Count !=
                        idsAsignaciones.Count)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "Una o más hospitalizaciones seleccionadas no existen."
                        });
                    }

                    foreach (var asignacion
                        in asignaciones)
                    {
                        if (asignacion.IdPaciente !=
                            idPaciente)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    "Una de las hospitalizaciones no pertenece al paciente de la factura."
                            });
                        }

                        if (!asignacion
                            .FechaEgreso.HasValue)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    "No se puede facturar una hospitalización que todavía no tiene fecha de egreso."
                            });
                        }

                        if (asignacion
                            .FechaEgreso.Value <=
                            asignacion.FechaIngreso)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    "La fecha de egreso de una hospitalización es inválida."
                            });
                        }

                        var yaFacturada =
                            await _context
                                .DetallesFactura
                                .AnyAsync(d =>
                                    d.IdAsignacionHabitacion ==
                                    asignacion.IdAsignacion);

                        if (yaFacturada)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    $"La hospitalización #{asignacion.IdAsignacion} ya fue facturada."
                            });
                        }

                        var tipoHabitacion =
                            asignacion.Habitacion
                                ?.TipoHabitacion;

                        if (tipoHabitacion == null)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    "Una de las hospitalizaciones no tiene tipo de habitación configurado."
                            });
                        }

                        var conceptoHospitalizacion =
                            await _context.Catalogos
                                .FirstOrDefaultAsync(c =>
                                    c.Estado &&
                                    c.IdTipoHabitacion ==
                                        tipoHabitacion
                                            .IdTipoHabitacion &&
                                    c.Tipo ==
                                        "Servicio");

                        if (conceptoHospitalizacion ==
                            null)
                        {
                            return BadRequest(new
                            {
                                mensaje =
                                    $"No existe un servicio activo en el catálogo para el tipo de habitación '{tipoHabitacion.Nombre}'."
                            });
                        }

                        var dias =
                            Math.Max(
                                1,
                                (int)Math.Ceiling(
                                    (asignacion
                                        .FechaEgreso
                                        .Value -
                                     asignacion
                                        .FechaIngreso)
                                    .TotalDays));

                        var precio =
                            tipoHabitacion
                                .PrecioPorDia;

                        detallesFactura.Add(
                            new DetalleFactura
                            {
                                IdCatalogo =
                                    conceptoHospitalizacion
                                        .IdCatalogo,

                                IdAsignacionHabitacion =
                                    asignacion
                                        .IdAsignacion,

                                Descripcion =
                                    $"{conceptoHospitalizacion.Nombre} ({dias} día(s))",

                                Cantidad =
                                    dias,

                                PrecioUnitario =
                                    precio,

                                Total =
                                    dias * precio
                            });
                    }
                }

                // Seguridad final.
                if (detallesFactura.Count == 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La factura debe contener al menos un concepto."
                    });
                }

                // =================================================
                // CREAR FACTURA
                // =================================================

                var totalFactura =
                    detallesFactura.Sum(d =>
                        d.Total);

                var factura =
                    new Factura
                    {
                        NumeroFactura =
                            $"TMP-{Guid.NewGuid().ToString("N")[..12]}",

                        IdPaciente =
                            request.IdPaciente,

                        IdUsuario =
                            idUsuario,

                        IdVenta =
                            idVenta,

                        FechaFactura =
                            ObtenerFechaGuatemala(),

                        Total =
                            totalFactura,

                        Estado =
                            "Emitida"
                    };

                _context.Facturas.Add(
                    factura);

                await _context.SaveChangesAsync();

                factura.NumeroFactura =
                    $"FAC-{factura.IdFactura:D6}";

                foreach (var detalle
                    in detallesFactura)
                {
                    detalle.IdFactura =
                        factura.IdFactura;
                }

                _context.DetallesFactura
                    .AddRange(
                        detallesFactura);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                var cantidadDetallesVenta =
                    tieneVenta
                        ? detallesFactura.Count(d =>
                            d.Descripcion.Contains(
                                " - Lote ",
                                StringComparison.Ordinal))
                        : 0;

                var origen =
                    !tieneVenta
                        ? "Clínica"
                        : detallesFactura.Count >
                          cantidadDetallesVenta
                            ? "Mixta"
                            : "Farmacia";

                return CreatedAtAction(
                    nameof(GetFactura),
                    new
                    {
                        id =
                            factura.IdFactura
                    },
                    new
                    {
                        factura.IdFactura,
                        factura.NumeroFactura,
                        factura.IdPaciente,

                        Paciente =
                            factura.IdPaciente.HasValue
                                ? "Paciente registrado"
                                : "Consumidor final",

                        factura.IdUsuario,
                        factura.IdVenta,
                        Origen = origen,
                        factura.FechaFactura,
                        factura.Total,
                        factura.Estado,

                        CantidadConceptos =
                            detallesFactura.Count
                    });
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                return Conflict(new
                {
                    mensaje =
                        "No fue posible guardar la factura. Verifica que la venta no haya sido facturada previamente."
                });
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // MODIFICAR ESTADO
        // =========================================================

        [HttpPut("{id}")]
        [Authorize(Policy = "FacturasModificar")]
        public async Task<IActionResult> ModificarFactura(
            int id,
            ActualizarEstadoFacturaDto request)
        {
            var factura =
                await _context.Facturas
                    .FindAsync(id);

            if (factura == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "La factura indicada no existe."
                });
            }

            var estado =
                (request.Estado ??
                 string.Empty)
                .Trim();

            var estadosPermitidos =
                new[]
                {
                    "Emitida",
                    "Pagada",
                    "Anulada"
                };

            if (!estadosPermitidos.Contains(
                    estado,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El estado debe ser Emitida, Pagada o Anulada."
                });
            }

            if (factura.Estado == "Anulada" &&
                !estado.Equals(
                    "Anulada",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Una factura anulada no puede volver a activarse."
                });
            }

            factura.Estado =
                estadosPermitidos.First(x =>
                    x.Equals(
                        estado,
                        StringComparison.OrdinalIgnoreCase));

            await _context.SaveChangesAsync();

            return Ok(new
            {
                factura.IdFactura,
                factura.NumeroFactura,
                factura.Estado
            });
        }

        // =========================================================
        // ELIMINAR
        //
        // SOLO PERMITE LIMPIAR FACTURAS VACIAS HEREDADAS.
        // LAS FACTURAS REALES SE ANULAN.
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize(Policy = "FacturasEliminar")]
        public async Task<IActionResult> EliminarFactura(
            int id)
        {
            var factura =
                await _context.Facturas
                    .Include(f =>
                        f.Detalles)
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            if (factura.Detalles.Any() ||
                factura.IdVenta != null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Una factura emitida no debe eliminarse. Cámbiala a estado Anulada."
                });
            }

            _context.Facturas.Remove(
                factura);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================================================
        // HORA LOCAL DE GUATEMALA
        // =========================================================

        private static DateTime ObtenerFechaGuatemala()
        {
            try
            {
                var zona =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        "America/Guatemala");

                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    zona);
            }
            catch
            {
                try
                {
                    var zona =
                        TimeZoneInfo.FindSystemTimeZoneById(
                            "Central America Standard Time");

                    return TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        zona);
                }
                catch
                {
                    return DateTime.Now;
                }
            }
        }
    }
}