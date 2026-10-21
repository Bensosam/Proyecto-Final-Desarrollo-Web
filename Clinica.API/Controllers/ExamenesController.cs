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

        private const int MaxPdfBytes =
            5 * 1024 * 1024;

        public ExamenesController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "ExamenesConsultar")]
        public async Task<IActionResult> GetExamenes()
        {
            var examenes =
                await _context.Examenes
                    .OrderByDescending(e => e.FechaExamen)
                    .Select(e => new
                    {
                        e.IdExamen,
                        e.IdDiagnostico,
                        e.IdConsulta,

                        IdPaciente =
                            e.Consulta != null
                                ? e.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            e.Consulta != null &&
                            e.Consulta.Paciente != null
                                ? e.Consulta.Paciente.Nombres +
                                  " " +
                                  e.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            e.Consulta != null
                                ? e.Consulta.NumeroConsultaPaciente
                                : 0,

                        Diagnostico =
                            e.Diagnostico != null
                                ? e.Diagnostico.DiagnosticoTexto
                                : null,

                        e.NombreExamen,
                        e.FechaExamen,
                        e.Resultado,
                        e.Observaciones,

                        TienePdf =
                            e.ArchivoPdf != null,

                        e.NombreArchivoPdf
                    })
                    .ToListAsync();

            return Ok(examenes);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ExamenesConsultar")]
        public async Task<IActionResult> GetExamen(
            int id)
        {
            var examen =
                await _context.Examenes
                    .Where(e =>
                        e.IdExamen == id)
                    .Select(e => new
                    {
                        e.IdExamen,
                        e.IdDiagnostico,
                        e.IdConsulta,

                        IdPaciente =
                            e.Consulta != null
                                ? e.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            e.Consulta != null &&
                            e.Consulta.Paciente != null
                                ? e.Consulta.Paciente.Nombres +
                                  " " +
                                  e.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            e.Consulta != null
                                ? e.Consulta.NumeroConsultaPaciente
                                : 0,

                        Diagnostico =
                            e.Diagnostico != null
                                ? e.Diagnostico.DiagnosticoTexto
                                : null,

                        e.NombreExamen,
                        e.FechaExamen,
                        e.Resultado,
                        e.Observaciones,

                        TienePdf =
                            e.ArchivoPdf != null,

                        e.NombreArchivoPdf
                    })
                    .FirstOrDefaultAsync();

            if (examen == null)
            {
                return NotFound();
            }

            return Ok(examen);
        }

        [HttpGet("{id}/pdf")]
        [Authorize(Policy = "ExamenesConsultar")]
        public async Task<IActionResult> GetPdf(
            int id)
        {
            var examen =
                await _context.Examenes
                    .Where(e =>
                        e.IdExamen == id)
                    .Select(e => new
                    {
                        e.ArchivoPdf,
                        e.NombreArchivoPdf
                    })
                    .FirstOrDefaultAsync();

            if (examen == null)
            {
                return NotFound();
            }

            if (examen.ArchivoPdf == null ||
                examen.ArchivoPdf.Length == 0)
            {
                return NotFound(new
                {
                    mensaje =
                        "Este examen no tiene un PDF adjunto."
                });
            }

            return Ok(new
            {
                NombreArchivo =
                    examen.NombreArchivoPdf
                    ?? "resultado.pdf",

                ContenidoBase64 =
                    Convert.ToBase64String(
                        examen.ArchivoPdf)
            });
        }

        [HttpPost]
        [Authorize(Policy = "ExamenesCrear")]
        public async Task<IActionResult> CrearExamen(
            Examen examen)
        {
            if (!examen.IdDiagnostico.HasValue)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe seleccionar el diagnóstico que origina el examen."
                });
            }

            var diagnostico =
                await _context.Diagnosticos
                    .Where(d =>
                        d.IdDiagnostico ==
                        examen.IdDiagnostico.Value)
                    .Select(d => new
                    {
                        d.IdConsulta
                    })
                    .FirstOrDefaultAsync();

            if (diagnostico == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El diagnóstico indicado no existe."
                });
            }

            examen.IdConsulta =
                diagnostico.IdConsulta;

            if (examen.FechaExamen == default)
            {
                examen.FechaExamen =
                    DateTime.Now;
            }

            var errorPdf =
                AplicarPdf(
                    examen,
                    examen);

            if (errorPdf != null)
            {
                return BadRequest(new
                {
                    mensaje = errorPdf
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
                    examen.IdDiagnostico,
                    examen.IdConsulta,
                    examen.NombreExamen,
                    examen.FechaExamen,
                    examen.Resultado,
                    examen.Observaciones,
                    examen.NombreArchivoPdf
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
                    mensaje =
                        "El ID de la URL no coincide con el ID del examen."
                });
            }

            var existente =
                await _context.Examenes
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (!examen.IdDiagnostico.HasValue)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe seleccionar el diagnóstico que origina el examen."
                });
            }

            var diagnostico =
                await _context.Diagnosticos
                    .Where(d =>
                        d.IdDiagnostico ==
                        examen.IdDiagnostico.Value)
                    .Select(d => new
                    {
                        d.IdConsulta
                    })
                    .FirstOrDefaultAsync();

            if (diagnostico == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El diagnóstico indicado no existe."
                });
            }

            existente.IdDiagnostico =
                examen.IdDiagnostico;

            existente.IdConsulta =
                diagnostico.IdConsulta;

            existente.NombreExamen =
                examen.NombreExamen;

            existente.FechaExamen =
                examen.FechaExamen;

            existente.Resultado =
                examen.Resultado;

            existente.Observaciones =
                examen.Observaciones;

            var errorPdf =
                AplicarPdf(
                    examen,
                    existente);

            if (errorPdf != null)
            {
                return BadRequest(new
                {
                    mensaje = errorPdf
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdExamen,
                existente.IdDiagnostico,
                existente.IdConsulta,
                existente.NombreExamen,
                existente.FechaExamen,
                existente.Resultado,
                existente.Observaciones,
                existente.NombreArchivoPdf
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ExamenesEliminar")]
        public async Task<IActionResult> EliminarExamen(
            int id)
        {
            var examen =
                await _context.Examenes
                    .FindAsync(id);

            if (examen == null)
            {
                return NotFound();
            }

            _context.Examenes.Remove(examen);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string? AplicarPdf(
            Examen origen,
            Examen destino)
        {
            if (string.IsNullOrWhiteSpace(
                origen.ArchivoPdfBase64))
            {
                return null;
            }

            byte[] bytes;

            try
            {
                bytes =
                    Convert.FromBase64String(
                        origen.ArchivoPdfBase64);
            }
            catch
            {
                return
                    "El archivo PDF recibido no es válido.";
            }

            if (bytes.Length >
                MaxPdfBytes)
            {
                return
                    "El PDF no puede superar los 5 MB.";
            }

            if (bytes.Length < 4 ||
                bytes[0] != 0x25 ||
                bytes[1] != 0x50 ||
                bytes[2] != 0x44 ||
                bytes[3] != 0x46)
            {
                return
                    "El archivo seleccionado no es un PDF válido.";
            }

            destino.ArchivoPdf =
                bytes;

            destino.NombreArchivoPdf =
                string.IsNullOrWhiteSpace(
                    origen.NombreArchivoPdf)
                    ? "resultado.pdf"
                    : Path.GetFileName(
                        origen.NombreArchivoPdf);

            return null;
        }
    }
}