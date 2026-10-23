using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotesMedicamentoController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public LotesMedicamentoController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "LotesMedicamentoConsultar")]
        public async Task<IActionResult> GetLotes()
        {
            var lotes =
                await _context.LotesMedicamento
                    .AsNoTracking()
                    .OrderByDescending(l =>
                        l.FechaIngreso)
                    .Select(l => new
                    {
                        l.IdLote,
                        l.IdMedicamento,

                        Medicamento =
                            l.Medicamento != null
                                ? l.Medicamento.Nombre
                                : null,

                        Marca =
                            l.Medicamento != null &&
                            l.Medicamento.Marca != null
                                ? l.Medicamento.Marca.Nombre
                                : null,

                        MedicamentoMarca =
                            l.Medicamento != null
                                ? l.Medicamento.Nombre +
                                  " — " +
                                  (l.Medicamento.Marca != null
                                      ? l.Medicamento.Marca.Nombre
                                      : "Sin marca")
                                : null,

                        l.NumeroLote,
                        l.FechaIngreso,
                        l.FechaVencimiento,
                        l.CantidadDisponible,
                        l.CostoUnitario,

                        ValorExistencia =
                            l.CantidadDisponible *
                            l.CostoUnitario
                    })
                    .ToListAsync();

            return Ok(lotes);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "LotesMedicamentoConsultar")]
        public async Task<IActionResult> GetLote(
            int id)
        {
            var lote =
                await _context.LotesMedicamento
                    .AsNoTracking()
                    .Where(l =>
                        l.IdLote == id)
                    .Select(l => new
                    {
                        l.IdLote,
                        l.IdMedicamento,

                        Medicamento =
                            l.Medicamento != null
                                ? l.Medicamento.Nombre
                                : null,

                        Marca =
                            l.Medicamento != null &&
                            l.Medicamento.Marca != null
                                ? l.Medicamento.Marca.Nombre
                                : null,

                        MedicamentoMarca =
                            l.Medicamento != null
                                ? l.Medicamento.Nombre +
                                  " — " +
                                  (l.Medicamento.Marca != null
                                      ? l.Medicamento.Marca.Nombre
                                      : "Sin marca")
                                : null,

                        l.NumeroLote,
                        l.FechaIngreso,
                        l.FechaVencimiento,
                        l.CantidadDisponible,
                        l.CostoUnitario,

                        ValorExistencia =
                            l.CantidadDisponible *
                            l.CostoUnitario
                    })
                    .FirstOrDefaultAsync();

            if (lote == null)
            {
                return NotFound();
            }

            return Ok(lote);
        }

        [HttpPost]
        [Authorize(Policy = "LotesMedicamentoCrear")]
        public async Task<IActionResult> CrearLote(
            LoteMedicamento lote)
        {
            var medicamentoExiste =
                await _context.Medicamentos
                    .AnyAsync(m =>
                        m.IdMedicamento ==
                            lote.IdMedicamento &&
                        m.Estado);

            if (!medicamentoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El medicamento indicado no existe o está inactivo."
                });
            }

            var error =
                ValidarLote(lote);

            if (error != null)
            {
                return BadRequest(new
                {
                    mensaje = error
                });
            }

            lote.NumeroLote =
                lote.NumeroLote.Trim();

            _context.LotesMedicamento.Add(
                lote);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLote),
                new
                {
                    id =
                        lote.IdLote
                },
                new
                {
                    lote.IdLote,
                    lote.IdMedicamento,
                    lote.NumeroLote,
                    lote.FechaIngreso,
                    lote.FechaVencimiento,
                    lote.CantidadDisponible,
                    lote.CostoUnitario
                });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "LotesMedicamentoModificar")]
        public async Task<IActionResult> ModificarLote(
            int id,
            LoteMedicamento lote)
        {
            if (id != lote.IdLote)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del lote."
                });
            }

            var existente =
                await _context.LotesMedicamento
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var medicamentoExiste =
                await _context.Medicamentos
                    .AnyAsync(m =>
                        m.IdMedicamento ==
                            lote.IdMedicamento &&
                        m.Estado);

            if (!medicamentoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El medicamento indicado no existe o está inactivo."
                });
            }

            var error =
                ValidarLote(lote);

            if (error != null)
            {
                return BadRequest(new
                {
                    mensaje = error
                });
            }

            existente.IdMedicamento =
                lote.IdMedicamento;

            existente.NumeroLote =
                lote.NumeroLote.Trim();

            existente.FechaIngreso =
                lote.FechaIngreso;

            existente.FechaVencimiento =
                lote.FechaVencimiento;

            existente.CantidadDisponible =
                lote.CantidadDisponible;

            existente.CostoUnitario =
                lote.CostoUnitario;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdLote,
                existente.IdMedicamento,
                existente.NumeroLote,
                existente.FechaIngreso,
                existente.FechaVencimiento,
                existente.CantidadDisponible,
                existente.CostoUnitario
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "LotesMedicamentoEliminar")]
        public async Task<IActionResult> EliminarLote(
            int id)
        {
            var lote =
                await _context.LotesMedicamento
                    .FindAsync(id);

            if (lote == null)
            {
                return NotFound();
            }

            var tieneMovimientos =
                await _context.MovimientosInventario
                    .AnyAsync(m =>
                        m.IdLote == id);

            var tieneVentas =
                await _context.DetallesVenta
                    .AnyAsync(d =>
                        d.IdLote == id);

            if (tieneMovimientos ||
                tieneVentas)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar el lote porque ya tiene movimientos de inventario o ventas asociadas."
                });
            }

            _context.LotesMedicamento.Remove(
                lote);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string? ValidarLote(
            LoteMedicamento lote)
        {
            if (string.IsNullOrWhiteSpace(
                    lote.NumeroLote))
            {
                return "El número de lote es obligatorio.";
            }

            if (lote.CantidadDisponible < 0)
            {
                return "La cantidad disponible no puede ser negativa.";
            }

            if (lote.CostoUnitario < 0)
            {
                return "El costo unitario no puede ser negativo.";
            }

            if (lote.FechaIngreso == default)
            {
                return "La fecha de ingreso es obligatoria.";
            }

            if (lote.FechaVencimiento == default)
            {
                return "La fecha de vencimiento es obligatoria.";
            }

            if (lote.FechaVencimiento.Date <
                lote.FechaIngreso.Date)
            {
                return "La fecha de vencimiento no puede ser anterior a la fecha de ingreso.";
            }

            return null;
        }
    }
}
