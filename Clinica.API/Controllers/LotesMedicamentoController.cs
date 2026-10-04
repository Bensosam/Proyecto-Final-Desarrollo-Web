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

        public LotesMedicamentoController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "LotesMedicamentoConsultar")]
        public async Task<IActionResult> GetLotes()
        {
            var lotes = await _context.LotesMedicamento
                .Include(l => l.Medicamento)
                .OrderByDescending(l => l.FechaIngreso)
                .Select(l => new
                {
                    l.IdLote,
                    l.IdMedicamento,
                    Medicamento = l.Medicamento != null
                        ? l.Medicamento.Nombre
                        : null,
                    l.NumeroLote,
                    l.FechaIngreso,
                    l.FechaVencimiento,
                    l.CantidadDisponible
                })
                .ToListAsync();

            return Ok(lotes);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "LotesMedicamentoConsultar")]
        public async Task<IActionResult> GetLote(int id)
        {
            var lote = await _context.LotesMedicamento
                .Include(l => l.Medicamento)
                .Where(l => l.IdLote == id)
                .Select(l => new
                {
                    l.IdLote,
                    l.IdMedicamento,
                    Medicamento = l.Medicamento != null
                        ? l.Medicamento.Nombre
                        : null,
                    l.NumeroLote,
                    l.FechaIngreso,
                    l.FechaVencimiento,
                    l.CantidadDisponible
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
        public async Task<IActionResult> CrearLote(LoteMedicamento lote)
        {
            var medicamentoExiste = await _context.Medicamentos
                .AnyAsync(m => m.IdMedicamento == lote.IdMedicamento);

            if (!medicamentoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El medicamento indicado no existe."
                });
            }

            _context.LotesMedicamento.Add(lote);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLote),
                new { id = lote.IdLote },
                new
                {
                    lote.IdLote,
                    lote.IdMedicamento,
                    lote.NumeroLote,
                    lote.FechaIngreso,
                    lote.FechaVencimiento,
                    lote.CantidadDisponible
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "LotesMedicamentoModificar")]
        public async Task<IActionResult> ModificarLote(
            int id,
            LoteMedicamento lote)
        {
            if (id != lote.IdLote)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del lote."
                });
            }

            var loteExistente = await _context.LotesMedicamento
                .FindAsync(id);

            if (loteExistente == null)
            {
                return NotFound();
            }

            var medicamentoExiste = await _context.Medicamentos
                .AnyAsync(m => m.IdMedicamento == lote.IdMedicamento);

            if (!medicamentoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El medicamento indicado no existe."
                });
            }

            loteExistente.IdMedicamento = lote.IdMedicamento;
            loteExistente.NumeroLote = lote.NumeroLote;
            loteExistente.FechaIngreso = lote.FechaIngreso;
            loteExistente.FechaVencimiento = lote.FechaVencimiento;
            loteExistente.CantidadDisponible = lote.CantidadDisponible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                loteExistente.IdLote,
                loteExistente.IdMedicamento,
                loteExistente.NumeroLote,
                loteExistente.FechaIngreso,
                loteExistente.FechaVencimiento,
                loteExistente.CantidadDisponible
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "LotesMedicamentoEliminar")]
        public async Task<IActionResult> EliminarLote(int id)
        {
            var lote = await _context.LotesMedicamento.FindAsync(id);

            if (lote == null)
            {
                return NotFound();
            }

            _context.LotesMedicamento.Remove(lote);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}