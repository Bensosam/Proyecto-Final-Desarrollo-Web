using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecetasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public RecetasController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "RecetasConsultar")]
        public async Task<IActionResult> GetRecetas()
        {
            var recetas =
                await _context.Recetas
                    .OrderByDescending(r => r.FechaEmision)
                    .Select(r => new
                    {
                        r.IdReceta,
                        r.IdConsulta,

                        IdPaciente =
                            r.Consulta != null
                                ? r.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            r.Consulta != null &&
                            r.Consulta.Paciente != null
                                ? r.Consulta.Paciente.Nombres +
                                  " " +
                                  r.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            r.Consulta != null
                                ? r.Consulta.NumeroConsultaPaciente
                                : 0,

                        r.FechaEmision,
                        r.Observaciones
                    })
                    .ToListAsync();

            return Ok(recetas);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "RecetasConsultar")]
        public async Task<IActionResult> GetReceta(
            int id)
        {
            var receta =
                await _context.Recetas
                    .Where(r =>
                        r.IdReceta == id)
                    .Select(r => new
                    {
                        r.IdReceta,
                        r.IdConsulta,

                        IdPaciente =
                            r.Consulta != null
                                ? r.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            r.Consulta != null &&
                            r.Consulta.Paciente != null
                                ? r.Consulta.Paciente.Nombres +
                                  " " +
                                  r.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            r.Consulta != null
                                ? r.Consulta.NumeroConsultaPaciente
                                : 0,

                        r.FechaEmision,
                        r.Observaciones
                    })
                    .FirstOrDefaultAsync();

            if (receta == null)
            {
                return NotFound();
            }

            return Ok(receta);
        }

        [HttpPost]
        [Authorize(Policy = "RecetasCrear")]
        public async Task<IActionResult> CrearReceta(
            Receta receta)
        {
            var consultaExiste =
                await _context.Consultas
                    .AnyAsync(c =>
                        c.IdConsulta ==
                        receta.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La consulta indicada no existe."
                });
            }

            if (receta.FechaEmision == default)
            {
                receta.FechaEmision =
                    DateTime.Now;
            }

            _context.Recetas.Add(receta);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetReceta),
                new { id = receta.IdReceta },
                receta);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RecetasModificar")]
        public async Task<IActionResult> ModificarReceta(
            int id,
            Receta receta)
        {
            if (id != receta.IdReceta)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID de la receta."
                });
            }

            var existente =
                await _context.Recetas
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var consultaExiste =
                await _context.Consultas
                    .AnyAsync(c =>
                        c.IdConsulta ==
                        receta.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La consulta indicada no existe."
                });
            }

            existente.IdConsulta =
                receta.IdConsulta;

            existente.FechaEmision =
                receta.FechaEmision;

            existente.Observaciones =
                receta.Observaciones;

            await _context.SaveChangesAsync();

            return Ok(existente);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RecetasEliminar")]
        public async Task<IActionResult> EliminarReceta(
            int id)
        {
            var receta =
                await _context.Recetas
                    .Include(r => r.Detalles)
                    .FirstOrDefaultAsync(r =>
                        r.IdReceta == id);

            if (receta == null)
            {
                return NotFound();
            }

            _context.Recetas.Remove(receta);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}