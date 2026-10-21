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

        public DiagnosticosController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: api/Diagnosticos
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "DiagnosticosConsultar")]
        public async Task<IActionResult> GetDiagnosticos()
        {
            var diagnosticos =
                await _context.Diagnosticos
                    .Include(d => d.Consulta)
                        .ThenInclude(c => c!.Paciente)
                    .OrderByDescending(d =>
                        d.FechaDiagnostico)
                    .Select(d => new
                    {
                        d.IdDiagnostico,

                        d.IdConsulta,

                        IdPaciente =
                            d.Consulta != null
                                ? d.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            d.Consulta != null &&
                            d.Consulta.Paciente != null
                                ? d.Consulta.Paciente.Nombres +
                                  " " +
                                  d.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            d.Consulta != null
                                ? d.Consulta.NumeroConsultaPaciente
                                : 0,

                        FechaConsulta =
                            d.Consulta != null
                                ? d.Consulta.FechaConsulta
                                : (DateTime?)null,

                        d.DiagnosticoTexto,
                        d.Observaciones,
                        d.FechaDiagnostico
                    })
                    .ToListAsync();

            return Ok(diagnosticos);
        }

        // =========================================================
        // GET: api/Diagnosticos/1
        // =========================================================

        [HttpGet("{id}")]
        [Authorize(Policy = "DiagnosticosConsultar")]
        public async Task<IActionResult> GetDiagnostico(
            int id)
        {
            var diagnostico =
                await _context.Diagnosticos
                    .Include(d => d.Consulta)
                        .ThenInclude(c => c!.Paciente)
                    .Where(d =>
                        d.IdDiagnostico == id)
                    .Select(d => new
                    {
                        d.IdDiagnostico,

                        d.IdConsulta,

                        IdPaciente =
                            d.Consulta != null
                                ? d.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            d.Consulta != null &&
                            d.Consulta.Paciente != null
                                ? d.Consulta.Paciente.Nombres +
                                  " " +
                                  d.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            d.Consulta != null
                                ? d.Consulta.NumeroConsultaPaciente
                                : 0,

                        FechaConsulta =
                            d.Consulta != null
                                ? d.Consulta.FechaConsulta
                                : (DateTime?)null,

                        d.DiagnosticoTexto,
                        d.Observaciones,
                        d.FechaDiagnostico
                    })
                    .FirstOrDefaultAsync();

            if (diagnostico == null)
            {
                return NotFound();
            }

            return Ok(diagnostico);
        }

        // =========================================================
        // POST: api/Diagnosticos
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "DiagnosticosCrear")]
        public async Task<IActionResult> CrearDiagnostico(
            Diagnostico diagnostico)
        {
            var consulta =
                await _context.Consultas
                    .Where(c =>
                        c.IdConsulta ==
                        diagnostico.IdConsulta)
                    .Select(c => new
                    {
                        c.IdConsulta,
                        c.IdPaciente,
                        c.FechaConsulta
                    })
                    .FirstOrDefaultAsync();

            if (consulta == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La consulta indicada no existe."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    diagnostico.DiagnosticoTexto))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El diagnóstico es obligatorio."
                });
            }

            if (diagnostico.FechaDiagnostico ==
                default)
            {
                diagnostico.FechaDiagnostico =
                    DateTime.Now;
            }

            _context.Diagnosticos.Add(
                diagnostico);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDiagnostico),
                new
                {
                    id =
                        diagnostico.IdDiagnostico
                },
                new
                {
                    diagnostico.IdDiagnostico,
                    diagnostico.IdConsulta,
                    consulta.IdPaciente,
                    diagnostico.DiagnosticoTexto,
                    diagnostico.Observaciones,
                    diagnostico.FechaDiagnostico
                });
        }

        // =========================================================
        // PUT: api/Diagnosticos/1
        // =========================================================

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
                    mensaje =
                        "El ID de la URL no coincide con el ID del diagnóstico."
                });
            }

            var diagnosticoExistente =
                await _context.Diagnosticos
                    .FindAsync(id);

            if (diagnosticoExistente == null)
            {
                return NotFound();
            }

            var consultaExiste =
                await _context.Consultas
                    .AnyAsync(c =>
                        c.IdConsulta ==
                        diagnostico.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La consulta indicada no existe."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    diagnostico.DiagnosticoTexto))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El diagnóstico es obligatorio."
                });
            }

            diagnosticoExistente.IdConsulta =
                diagnostico.IdConsulta;

            diagnosticoExistente.DiagnosticoTexto =
                diagnostico.DiagnosticoTexto;

            diagnosticoExistente.Observaciones =
                diagnostico.Observaciones;

            diagnosticoExistente.FechaDiagnostico =
                diagnostico.FechaDiagnostico == default
                    ? diagnosticoExistente.FechaDiagnostico
                    : diagnostico.FechaDiagnostico;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                diagnosticoExistente.IdDiagnostico,
                diagnosticoExistente.IdConsulta,
                diagnosticoExistente.DiagnosticoTexto,
                diagnosticoExistente.Observaciones,
                diagnosticoExistente.FechaDiagnostico
            });
        }

        // =========================================================
        // DELETE: api/Diagnosticos/1
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize(Policy = "DiagnosticosEliminar")]
        public async Task<IActionResult> EliminarDiagnostico(
            int id)
        {
            var diagnostico =
                await _context.Diagnosticos
                    .FindAsync(id);

            if (diagnostico == null)
            {
                return NotFound();
            }

            _context.Diagnosticos.Remove(
                diagnostico);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}