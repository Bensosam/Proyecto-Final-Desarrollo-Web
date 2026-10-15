using Clinica.API.Data;
using Clinica.API.Models;
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

        public DetalleFacturaController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpPost("hospitalizacion/{idFactura}/{idAsignacion}")]
        [Authorize(Policy = "DetalleFacturaCrear")]
        public async Task<IActionResult> CrearHospitalizacion(
            int idFactura,
            int idAsignacion)
        {
            var factura = await _context.Facturas
                .FindAsync(idFactura);

            if (factura == null)
            {
                return BadRequest(new
                {
                    mensaje = "La factura indicada no existe."
                });
            }

            if (factura.Estado == "Anulada")
            {
                return BadRequest(new
                {
                    mensaje = "No se puede agregar hospitalización a una factura anulada."
                });
            }

            var asignacion = await _context.AsignacionesHabitacion
                .Include(a => a.Habitacion)
                    .ThenInclude(h => h!.TipoHabitacion)
                .FirstOrDefaultAsync(a =>
                    a.IdAsignacion == idAsignacion);

            if (asignacion == null)
            {
                return BadRequest(new
                {
                    mensaje = "La asignación de habitación indicada no existe."
                });
            }

            if (asignacion.IdPaciente != factura.IdPaciente)
            {
                return BadRequest(new
                {
                    mensaje = "La asignación de habitación no pertenece al paciente de la factura."
                });
            }

            if (asignacion.FechaEgreso == null)
            {
                return BadRequest(new
                {
                    mensaje = "La hospitalización todavía no tiene fecha de egreso."
                });
            }

            if (asignacion.FechaEgreso <= asignacion.FechaIngreso)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha de egreso debe ser posterior a la fecha de ingreso."
                });
            }

            if (asignacion.Habitacion?.TipoHabitacion == null)
            {
                return BadRequest(new
                {
                    mensaje = "La habitación no tiene un tipo de habitación asociado."
                });
            }

                 var yaFacturada = await _context.DetallesFactura
                .AnyAsync(d =>
                    d.IdAsignacionHabitacion == idAsignacion);

            if (yaFacturada)
            {
                return BadRequest(new
                {
                    mensaje = "Esta hospitalización ya fue facturada anteriormente."
                });
            }

            var tipoHabitacion =
                asignacion.Habitacion.TipoHabitacion;

            var itemCatalogo = await _context.Catalogos
                .FirstOrDefaultAsync(c =>
                    c.IdTipoHabitacion ==
                        tipoHabitacion.IdTipoHabitacion &&
                    c.Tipo == "Servicio" &&
                    c.Estado);

            if (itemCatalogo == null)
            {
                return BadRequest(new
                {
                    mensaje = "No existe un servicio activo en el catálogo para este tipo de habitación."
                });
            }

            var tiempoHospitalizado =
                asignacion.FechaEgreso.Value -
                asignacion.FechaIngreso;

            var dias = Math.Max(
                1,
                (int)Math.Ceiling(
                    tiempoHospitalizado.TotalDays));

            var detalle = new DetalleFactura
            {
                IdFactura = factura.IdFactura,
                IdCatalogo = itemCatalogo.IdCatalogo,
                IdAsignacionHabitacion =
                    asignacion.IdAsignacion,
                Descripcion =
                    $"{itemCatalogo.Nombre} ({dias} día(s))",
                Cantidad = dias,
                PrecioUnitario =
                    tipoHabitacion.PrecioPorDia,
                Total =
                    dias * tipoHabitacion.PrecioPorDia
            };

            factura.Total += detalle.Total;

            _context.DetallesFactura.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDetalle),
                new { id = detalle.IdDetalleFactura },
                new
                {
                    detalle.IdDetalleFactura,
                    detalle.IdFactura,
                    detalle.IdCatalogo,
                    detalle.IdAsignacionHabitacion,
                    detalle.Descripcion,
                    FechaIngreso = asignacion.FechaIngreso,
                    FechaEgreso = asignacion.FechaEgreso,
                    DiasFacturados = dias,
                    detalle.PrecioUnitario,
                    detalle.Total,
                    TotalFactura = factura.Total
                });
        }

        [HttpGet]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await _context.DetallesFactura
                .Include(d => d.Catalogo)
                .OrderBy(d => d.IdDetalleFactura)
                .Select(d => new
                {
                    d.IdDetalleFactura,
                    d.IdFactura,
                    d.IdCatalogo,
                    Codigo = d.Catalogo != null
                        ? d.Catalogo.Codigo
                        : null,
                    d.Descripcion,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var detalle = await _context.DetallesFactura
                .Include(d => d.Catalogo)
                .Where(d => d.IdDetalleFactura == id)
                .Select(d => new
                {
                    d.IdDetalleFactura,
                    d.IdFactura,
                    d.IdCatalogo,
                    Codigo = d.Catalogo != null
                        ? d.Catalogo.Codigo
                        : null,
                    d.Descripcion,
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

        [HttpGet("factura/{idFactura}")]
        [Authorize(Policy = "DetalleFacturaConsultar")]
        public async Task<IActionResult> GetPorFactura(int idFactura)
        {
            var facturaExiste = await _context.Facturas
                .AnyAsync(f => f.IdFactura == idFactura);

            if (!facturaExiste)
            {
                return NotFound(new
                {
                    mensaje = "La factura indicada no existe."
                });
            }

            var detalles = await _context.DetallesFactura
                .Include(d => d.Catalogo)
                .Where(d => d.IdFactura == idFactura)
                .OrderBy(d => d.IdDetalleFactura)
                .Select(d => new
                {
                    d.IdDetalleFactura,
                    d.IdFactura,
                    d.IdCatalogo,
                    Codigo = d.Catalogo != null
                        ? d.Catalogo.Codigo
                        : null,
                    d.Descripcion,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpPost]
        [Authorize(Policy = "DetalleFacturaCrear")]
        public async Task<IActionResult> CrearDetalle(
            DetalleFactura detalle)
        {
            var factura = await _context.Facturas
                .FindAsync(detalle.IdFactura);

            if (factura == null)
            {
                return BadRequest(new
                {
                    mensaje = "La factura indicada no existe."
                });
            }

            if (factura.Estado == "Anulada")
            {
                return BadRequest(new
                {
                    mensaje = "No se pueden agregar detalles a una factura anulada."
                });
            }

            var item = await _context.Catalogos
                .FirstOrDefaultAsync(c =>
                    c.IdCatalogo == detalle.IdCatalogo &&
                    c.Estado);

            if (item == null)
            {
                return BadRequest(new
                {
                    mensaje = "El elemento del catálogo no existe o está inactivo."
                });
            }

            if (item.IdTipoHabitacion != null)
            {
                return BadRequest(new
                {
                    mensaje = "Los servicios de hospitalización deben facturarse usando la asignación de habitación."
                });
            }

            if (detalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            detalle.Descripcion = item.Nombre;
            detalle.PrecioUnitario = item.Precio;
            detalle.Total =
                detalle.Cantidad * detalle.PrecioUnitario;

            factura.Total += detalle.Total;

            _context.DetallesFactura.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDetalle),
                new { id = detalle.IdDetalleFactura },
                new
                {
                    detalle.IdDetalleFactura,
                    detalle.IdFactura,
                    detalle.IdCatalogo,
                    detalle.Descripcion,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.Total,
                    TotalFactura = factura.Total
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "DetalleFacturaModificar")]
        public async Task<IActionResult> ModificarDetalle(
            int id,
            DetalleFactura detalle)
        {
            if (id != detalle.IdDetalleFactura)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del detalle."
                });
            }

            var existente = await _context.DetallesFactura
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (detalle.IdFactura != existente.IdFactura)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede cambiar la factura asociada al detalle."
                });
            }

            var factura = await _context.Facturas
                .FindAsync(existente.IdFactura);

            if (factura == null)
            {
                return BadRequest(new
                {
                    mensaje = "La factura asociada no existe."
                });
            }

            if (factura.Estado == "Anulada")
            {
                return BadRequest(new
                {
                    mensaje = "No se puede modificar una factura anulada."
                });
            }

            var item = await _context.Catalogos
                .FirstOrDefaultAsync(c =>
                    c.IdCatalogo == detalle.IdCatalogo &&
                    c.Estado);

            if (item == null)
            {
                return BadRequest(new
                {
                    mensaje = "El elemento del catálogo no existe o está inactivo."
                });
            }
            if (existente.IdAsignacionHabitacion != null)
            {
                return BadRequest(new
                {
                    mensaje = "Los detalles de hospitalización no pueden modificarse manualmente."
                });
            }

            if (item.IdTipoHabitacion != null)
            {
                return BadRequest(new
                {
                    mensaje = "Los servicios de hospitalización deben facturarse usando la asignación de habitación."
                });
            }


            if (detalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            var totalAnterior = existente.Total;

            existente.IdCatalogo = detalle.IdCatalogo;
            existente.Descripcion = item.Nombre;
            existente.Cantidad = detalle.Cantidad;
            existente.PrecioUnitario = item.Precio;
            existente.Total =
                existente.Cantidad * existente.PrecioUnitario;

            factura.Total =
                factura.Total -
                totalAnterior +
                existente.Total;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdDetalleFactura,
                existente.IdFactura,
                existente.IdCatalogo,
                existente.Descripcion,
                existente.Cantidad,
                existente.PrecioUnitario,
                existente.Total,
                TotalFactura = factura.Total
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DetalleFacturaEliminar")]
        public async Task<IActionResult> EliminarDetalle(int id)
        {
            var detalle = await _context.DetallesFactura
                .FindAsync(id);

            if (detalle == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .FindAsync(detalle.IdFactura);

            if (factura != null)
            {
                if (factura.Estado == "Anulada")
                {
                    return BadRequest(new
                    {
                        mensaje = "No se puede modificar una factura anulada."
                    });
                }

                factura.Total -= detalle.Total;

                if (factura.Total < 0)
                {
                    factura.Total = 0;
                }
            }

            _context.DetallesFactura.Remove(detalle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}