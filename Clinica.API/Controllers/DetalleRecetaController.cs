using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleRecetaController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public DetalleRecetaController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "DetalleRecetaConsultar")]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await _context.DetallesReceta
                .Include(d => d.Medicamento)
                .OrderBy(d => d.IdDetalleReceta)
                .Select(d => new
                {
                    d.IdDetalleReceta,
                    d.IdReceta,
                    d.IdMedicamento,
                    Medicamento = d.Medicamento != null
                        ? d.Medicamento.Nombre
                        : null,
                    d.Cantidad,
                    d.Dosis,
                    d.Frecuencia,
                    d.DuracionDias,
                    d.Indicaciones
                })
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "DetalleRecetaConsultar")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var detalle = await _context.DetallesReceta
                .Include(d => d.Medicamento)
                .Where(d => d.IdDetalleReceta == id)
                .Select(d => new
                {
                    d.IdDetalleReceta,
                    d.IdReceta,
                    d.IdMedicamento,
                    Medicamento = d.Medicamento != null
                        ? d.Medicamento.Nombre
                        : null,
                    d.Cantidad,
                    d.Dosis,
                    d.Frecuencia,
                    d.DuracionDias,
                    d.Indicaciones
                })
                .FirstOrDefaultAsync();

            if (detalle == null)
                return NotFound();

            return Ok(detalle);
        }

        [HttpGet("receta/{idReceta}")]
        [Authorize(Policy = "DetalleRecetaConsultar")]
        public async Task<IActionResult> GetPorReceta(int idReceta)
        {
            var recetaExiste = await _context.Recetas
                .AnyAsync(r => r.IdReceta == idReceta);

            if (!recetaExiste)
            {
                return NotFound(new
                {
                    mensaje = "La receta indicada no existe."
                });
            }

            var detalles = await _context.DetallesReceta
                .Include(d => d.Medicamento)
                .Where(d => d.IdReceta == idReceta)
                .Select(d => new
                {
                    d.IdDetalleReceta,
                    d.IdReceta,
                    d.IdMedicamento,
                    Medicamento = d.Medicamento != null
                        ? d.Medicamento.Nombre
                        : null,
                    d.Cantidad,
                    d.Dosis,
                    d.Frecuencia,
                    d.DuracionDias,
                    d.Indicaciones
                })
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpPost]
        [Authorize(Policy = "DetalleRecetaCrear")]
        public async Task<IActionResult> CrearDetalle(DetalleReceta detalle)
        {
            var recetaExiste = await _context.Recetas
                .AnyAsync(r => r.IdReceta == detalle.IdReceta);

            if (!recetaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La receta indicada no existe."
                });
            }

            var medicamentoExiste = await _context.Medicamentos
                .AnyAsync(m => m.IdMedicamento == detalle.IdMedicamento);

            if (!medicamentoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El medicamento indicado no existe."
                });
            }

            if (detalle.Cantidad <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad debe ser mayor que cero."
                });
            }

            if (detalle.DuracionDias <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La duración debe ser mayor que cero."
                });
            }

            _context.DetallesReceta.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDetalle),
                new { id = detalle.IdDetalleReceta },
                new
                {
                    detalle.IdDetalleReceta,
                    detalle.IdReceta,
                    detalle.IdMedicamento,
                    detalle.Cantidad,
                    detalle.Dosis,
                    detalle.Frecuencia,
                    detalle.DuracionDias,
                    detalle.Indicaciones
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "DetalleRecetaModificar")]
        public async Task<IActionResult> ModificarDetalle(
            int id,
            DetalleReceta detalle)
        {
            if (id != detalle.IdDetalleReceta)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del detalle."
                });
            }

            var detalleExistente = await _context.DetallesReceta
                .FindAsync(id);

            if (detalleExistente == null)
                return NotFound();

            var recetaExiste = await _context.Recetas
                .AnyAsync(r => r.IdReceta == detalle.IdReceta);

            if (!recetaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La receta indicada no existe."
                });
            }

            var medicamentoExiste = await _context.Medicamentos
                .AnyAsync(m => m.IdMedicamento == detalle.IdMedicamento);

            if (!medicamentoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El medicamento indicado no existe."
                });
            }

            if (detalle.Cantidad <= 0 || detalle.DuracionDias <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Cantidad y duración deben ser mayores que cero."
                });
            }

            detalleExistente.IdReceta = detalle.IdReceta;
            detalleExistente.IdMedicamento = detalle.IdMedicamento;
            detalleExistente.Cantidad = detalle.Cantidad;
            detalleExistente.Dosis = detalle.Dosis;
            detalleExistente.Frecuencia = detalle.Frecuencia;
            detalleExistente.DuracionDias = detalle.DuracionDias;
            detalleExistente.Indicaciones = detalle.Indicaciones;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                detalleExistente.IdDetalleReceta,
                detalleExistente.IdReceta,
                detalleExistente.IdMedicamento,
                detalleExistente.Cantidad,
                detalleExistente.Dosis,
                detalleExistente.Frecuencia,
                detalleExistente.DuracionDias,
                detalleExistente.Indicaciones
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DetalleRecetaEliminar")]
        public async Task<IActionResult> EliminarDetalle(int id)
        {
            var detalle = await _context.DetallesReceta.FindAsync(id);

            if (detalle == null)
                return NotFound();

            _context.DetallesReceta.Remove(detalle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}