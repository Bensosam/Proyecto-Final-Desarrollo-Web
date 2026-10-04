using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public DiagnosticosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "DiagnosticosConsultar")]
        public async Task<IActionResult> GetDiagnosticos()
        {
            var diagnosticos = await _context.Diagnosticos
                .OrderByDescending(d => d.IdDiagnostico)
                .Select(d => new
                {
                    d.IdDiagnostico,
                    d.IdConsulta,
                    d.DiagnosticoTexto,
                    d.Observaciones
                })
                .ToListAsync();

            return Ok(diagnosticos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "DiagnosticosConsultar")]
        public async Task<IActionResult> GetDiagnostico(int id)
        {
            var diagnostico = await _context.Diagnosticos
                .Where(d => d.IdDiagnostico == id)
                .Select(d => new
                {
                    d.IdDiagnostico,
                    d.IdConsulta,
                    d.DiagnosticoTexto,
                    d.Observaciones
                })
                .FirstOrDefaultAsync();

            if (diagnostico == null)
            {
                return NotFound();
            }

            return Ok(diagnostico);
        }

        [HttpPost]
        [Authorize(Policy = "DiagnosticosCrear")]
        public async Task<IActionResult> CrearDiagnostico(Diagnostico diagnostico)
        {
            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == diagnostico.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            _context.Diagnosticos.Add(diagnostico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDiagnostico),
                new { id = diagnostico.IdDiagnostico },
                new
                {
                    diagnostico.IdDiagnostico,
                    diagnostico.IdConsulta,
                    diagnostico.DiagnosticoTexto,
                    diagnostico.Observaciones
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "DiagnosticosModificar")]
        public async Task<IActionResult> ModificarDiagnostico(
            int id,
            Diagnostico diagnostico)
        {
            if (id != diagnostico.IdDiagnostico)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del diagnóstico."
                });
            }

            var diagnosticoExistente = await _context.Diagnosticos
                .FindAsync(id);

            if (diagnosticoExistente == null)
            {
                return NotFound();
            }

            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == diagnostico.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            diagnosticoExistente.IdConsulta = diagnostico.IdConsulta;
            diagnosticoExistente.DiagnosticoTexto = diagnostico.DiagnosticoTexto;
            diagnosticoExistente.Observaciones = diagnostico.Observaciones;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                diagnosticoExistente.IdDiagnostico,
                diagnosticoExistente.IdConsulta,
                diagnosticoExistente.DiagnosticoTexto,
                diagnosticoExistente.Observaciones
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DiagnosticosEliminar")]
        public async Task<IActionResult> EliminarDiagnostico(int id)
        {
            var diagnostico = await _context.Diagnosticos
                .FindAsync(id);

            if (diagnostico == null)
            {
                return NotFound();
            }

            _context.Diagnosticos.Remove(diagnostico);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}