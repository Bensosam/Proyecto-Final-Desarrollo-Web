using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public ExamenesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "ExamenesConsultar")]
        public async Task<IActionResult> GetExamenes()
        {
            var examenes = await _context.Examenes
                .OrderByDescending(e => e.IdExamen)
                .Select(e => new
                {
                    e.IdExamen,
                    e.IdConsulta,
                    e.NombreExamen,
                    e.FechaExamen,
                    e.Resultado,
                    e.Observaciones
                })
                .ToListAsync();

            return Ok(examenes);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ExamenesConsultar")]
        public async Task<IActionResult> GetExamen(int id)
        {
            var examen = await _context.Examenes
                .Where(e => e.IdExamen == id)
                .Select(e => new
                {
                    e.IdExamen,
                    e.IdConsulta,
                    e.NombreExamen,
                    e.FechaExamen,
                    e.Resultado,
                    e.Observaciones
                })
                .FirstOrDefaultAsync();

            if (examen == null)
            {
                return NotFound();
            }

            return Ok(examen);
        }

        [HttpPost]
        [Authorize(Policy = "ExamenesCrear")]
        public async Task<IActionResult> CrearExamen(Examen examen)
        {
            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == examen.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            _context.Examenes.Add(examen);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetExamen),
                new { id = examen.IdExamen },
                new
                {
                    examen.IdExamen,
                    examen.IdConsulta,
                    examen.NombreExamen,
                    examen.FechaExamen,
                    examen.Resultado,
                    examen.Observaciones
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ExamenesModificar")]
        public async Task<IActionResult> ModificarExamen(
            int id,
            Examen examen)
        {
            if (id != examen.IdExamen)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del examen."
                });
            }

            var examenExistente = await _context.Examenes
                .FindAsync(id);

            if (examenExistente == null)
            {
                return NotFound();
            }

            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == examen.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            examenExistente.IdConsulta = examen.IdConsulta;
            examenExistente.NombreExamen = examen.NombreExamen;
            examenExistente.FechaExamen = examen.FechaExamen;
            examenExistente.Resultado = examen.Resultado;
            examenExistente.Observaciones = examen.Observaciones;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                examenExistente.IdExamen,
                examenExistente.IdConsulta,
                examenExistente.NombreExamen,
                examenExistente.FechaExamen,
                examenExistente.Resultado,
                examenExistente.Observaciones
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ExamenesEliminar")]
        public async Task<IActionResult> EliminarExamen(int id)
        {
            var examen = await _context.Examenes.FindAsync(id);

            if (examen == null)
            {
                return NotFound();
            }

            _context.Examenes.Remove(examen);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}