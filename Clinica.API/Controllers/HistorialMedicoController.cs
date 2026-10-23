using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialMedicoController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public HistorialMedicoController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // HISTORIAL COMPLETO DEL PACIENTE
        // =========================================================

        [HttpGet("paciente/{idPaciente:int}")]
        [Authorize(Policy = "PacientesConsultar")]
        public async Task<IActionResult> GetHistorialPaciente(
            int idPaciente)
        {
            var paciente =
                await _context.Pacientes
                    .AsNoTracking()
                    .Where(p =>
                        p.IdPaciente == idPaciente)
                    .Select(p => new
                    {
                        p.IdPaciente,
                        p.Nombres,
                        p.Apellidos,
                        p.DPI,
                        p.FechaNacimiento,
                        p.Genero,
                        p.Direccion,
                        p.Telefono,
                        p.Correo,
                        p.FechaRegistro,
                        p.Estado
                    })
                    .FirstOrDefaultAsync();

            if (paciente == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El paciente indicado no existe."
                });
            }

            var historial =
                await ObtenerOCrearHistorialAsync(
                    idPaciente);

            // -----------------------------------------------------
            // CONSULTAS
            // -----------------------------------------------------

            var consultas =
                await _context.Consultas
                    .AsNoTracking()
                    .Where(c =>
                        c.IdPaciente == idPaciente)
                    .OrderByDescending(c =>
                        c.FechaConsulta)
                    .Select(c => new
                    {
                        c.IdConsulta,
                        c.NumeroConsultaPaciente,
                        c.FechaConsulta,
                        c.LecturaResultadosExamenes,
                        c.Motivo,
                        c.Observaciones,
                        c.IdEmpleado,

                        Profesional =
                            c.Empleado != null
                                ? c.Empleado.Nombres +
                                  " " +
                                  c.Empleado.Apellidos
                                : null
                    })
                    .ToListAsync();

            // -----------------------------------------------------
            // DIAGNOSTICOS
            // -----------------------------------------------------

            var diagnosticos =
                await _context.Diagnosticos
                    .AsNoTracking()
                    .Where(d =>
                        d.Consulta != null &&
                        d.Consulta.IdPaciente ==
                            idPaciente)
                    .OrderByDescending(d =>
                        d.FechaDiagnostico)
                    .Select(d => new
                    {
                        d.IdDiagnostico,
                        d.IdConsulta,

                        NumeroConsultaPaciente =
                            d.Consulta != null
                                ? d.Consulta
                                    .NumeroConsultaPaciente
                                : 0,

                        d.DiagnosticoTexto,
                        d.Observaciones,
                        d.FechaDiagnostico
                    })
                    .ToListAsync();

            // -----------------------------------------------------
            // EXAMENES
            // -----------------------------------------------------

            var examenes =
                await _context.Examenes
                    .AsNoTracking()
                    .Where(e =>
                        e.Consulta != null &&
                        e.Consulta.IdPaciente ==
                            idPaciente)
                    .OrderByDescending(e =>
                        e.FechaExamen)
                    .Select(e => new
                    {
                        e.IdExamen,
                        e.IdConsulta,
                        e.IdDiagnostico,

                        NumeroConsultaPaciente =
                            e.Consulta != null
                                ? e.Consulta
                                    .NumeroConsultaPaciente
                                : 0,

                        Diagnostico =
                            e.Diagnostico != null
                                ? e.Diagnostico
                                    .DiagnosticoTexto
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

            // -----------------------------------------------------
            // RECETAS
            // -----------------------------------------------------

            var recetas =
                await _context.Recetas
                    .AsNoTracking()
                    .Where(r =>
                        r.Consulta != null &&
                        r.Consulta.IdPaciente ==
                            idPaciente)
                    .OrderByDescending(r =>
                        r.FechaEmision)
                    .Select(r => new
                    {
                        r.IdReceta,
                        r.IdConsulta,

                        NumeroConsultaPaciente =
                            r.Consulta != null
                                ? r.Consulta
                                    .NumeroConsultaPaciente
                                : 0,

                        r.FechaEmision,
                        r.Observaciones
                    })
                    .ToListAsync();

            // -----------------------------------------------------
            // MEDICAMENTOS RECETADOS
            // -----------------------------------------------------

            var medicamentosRecetados =
                await _context.DetallesReceta
                    .AsNoTracking()
                    .Where(d =>
                        d.Receta != null &&
                        d.Receta.Consulta != null &&
                        d.Receta.Consulta.IdPaciente ==
                            idPaciente)
                    .OrderByDescending(d =>
                        d.IdDetalleReceta)
                    .Select(d => new
                    {
                        d.IdDetalleReceta,
                        d.IdReceta,
                        d.IdMedicamento,

                        Medicamento =
                            d.Medicamento != null
                                ? d.Medicamento.Nombre
                                : null,

                        d.Cantidad,
                        d.Dosis,
                        d.Frecuencia,
                        d.DuracionDias,
                        d.Indicaciones
                    })
                    .ToListAsync();

            // -----------------------------------------------------
            // TRATAMIENTOS
            // -----------------------------------------------------

            var tratamientos =
                await _context.Tratamientos
                    .AsNoTracking()
                    .Where(t =>
                        t.Consulta != null &&
                        t.Consulta.IdPaciente ==
                            idPaciente)
                    .OrderByDescending(t =>
                        t.FechaInicio)
                    .Select(t => new
                    {
                        t.IdTratamiento,
                        t.IdConsulta,

                        NumeroConsultaPaciente =
                            t.Consulta != null
                                ? t.Consulta
                                    .NumeroConsultaPaciente
                                : 0,

                        t.Descripcion,
                        t.Indicaciones,
                        t.FechaInicio,
                        t.FechaFin
                    })
                    .ToListAsync();

            // -----------------------------------------------------
            // EVOLUCIONES
            // -----------------------------------------------------

            var evoluciones =
                await _context.Evoluciones
                    .AsNoTracking()
                    .Where(e =>
                        e.Consulta != null &&
                        e.Consulta.IdPaciente ==
                            idPaciente)
                    .OrderByDescending(e =>
                        e.Fecha)
                    .Select(e => new
                    {
                        e.IdEvolucion,
                        e.IdConsulta,
                        e.IdReceta,
                        e.IdTratamiento,

                        NumeroConsultaPaciente =
                            e.Consulta != null
                                ? e.Consulta
                                    .NumeroConsultaPaciente
                                : 0,

                        e.IdEmpleado,

                        Profesional =
                            e.Empleado != null
                                ? e.Empleado.Nombres +
                                  " " +
                                  e.Empleado.Apellidos
                                : null,

                        e.Fecha,
                        e.Descripcion
                    })
                    .ToListAsync();

            // -----------------------------------------------------
            // HOSPITALIZACIONES
            // -----------------------------------------------------

            var hospitalizaciones =
                await _context.AsignacionesHabitacion
                    .AsNoTracking()
                    .Where(a =>
                        a.IdPaciente == idPaciente)
                    .OrderByDescending(a =>
                        a.FechaIngreso)
                    .Select(a => new
                    {
                        a.IdAsignacion,
                        a.IdDiagnostico,

                        Diagnostico =
                            a.Diagnostico != null
                                ? a.Diagnostico
                                    .DiagnosticoTexto
                                : null,

                        a.IdHabitacion,

                        Habitacion =
                            a.Habitacion != null
                                ? a.Habitacion.Numero
                                : null,

                        TipoHabitacion =
                            a.Habitacion != null &&
                            a.Habitacion.TipoHabitacion != null
                                ? a.Habitacion
                                    .TipoHabitacion.Nombre
                                : null,

                        a.FechaIngreso,
                        a.FechaEgreso,
                        a.Estado
                    })
                    .ToListAsync();

            return Ok(new
            {
                Paciente =
                    paciente,

                EntrevistaInicial =
                    new
                    {
                        historial.IdHistorialMedico,
                        historial.IdPaciente,
                        historial.FechaCreacion,
                        historial.AntecedentesPersonales,
                        historial.AntecedentesFamiliares,
                        historial.Alergias,
                        historial.EnfermedadesPrevias,
                        historial.Cirugias,
                        historial.MedicamentosHabituales,
                        historial.Habitos,
                        historial.ObservacionesEntrevistaInicial
                    },

                Consultas =
                    consultas,

                Diagnosticos =
                    diagnosticos,

                Examenes =
                    examenes,

                Recetas =
                    recetas,

                MedicamentosRecetados =
                    medicamentosRecetados,

                Tratamientos =
                    tratamientos,

                Evoluciones =
                    evoluciones,

                Hospitalizaciones =
                    hospitalizaciones
            });
        }

        // =========================================================
        // GUARDAR ENTREVISTA INICIAL
        // =========================================================

        [HttpPut("paciente/{idPaciente:int}/entrevista")]
        [Authorize(Policy = "PacientesModificar")]
        public async Task<IActionResult> GuardarEntrevistaInicial(
            int idPaciente,
            HistorialMedico datos)
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

            var historial =
                await ObtenerOCrearHistorialAsync(
                    idPaciente);

            historial.AntecedentesPersonales =
                datos.AntecedentesPersonales
                    ?.Trim()
                ?? string.Empty;

            historial.AntecedentesFamiliares =
                datos.AntecedentesFamiliares
                    ?.Trim()
                ?? string.Empty;

            historial.Alergias =
                datos.Alergias?.Trim()
                ?? string.Empty;

            historial.EnfermedadesPrevias =
                datos.EnfermedadesPrevias
                    ?.Trim()
                ?? string.Empty;

            historial.Cirugias =
                datos.Cirugias?.Trim()
                ?? string.Empty;

            historial.MedicamentosHabituales =
                datos.MedicamentosHabituales
                    ?.Trim()
                ?? string.Empty;

            historial.Habitos =
                datos.Habitos?.Trim()
                ?? string.Empty;

            historial.ObservacionesEntrevistaInicial =
                datos.ObservacionesEntrevistaInicial
                    ?.Trim()
                ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                historial.IdHistorialMedico,
                historial.IdPaciente,
                historial.FechaCreacion,
                historial.AntecedentesPersonales,
                historial.AntecedentesFamiliares,
                historial.Alergias,
                historial.EnfermedadesPrevias,
                historial.Cirugias,
                historial.MedicamentosHabituales,
                historial.Habitos,
                historial.ObservacionesEntrevistaInicial
            });
        }

        // =========================================================
        // COMPATIBILIDAD CON PACIENTES ANTIGUOS
        // =========================================================

        private async Task<HistorialMedico>
            ObtenerOCrearHistorialAsync(
                int idPaciente)
        {
            var historial =
                await _context.HistorialesMedicos
                    .FirstOrDefaultAsync(h =>
                        h.IdPaciente ==
                            idPaciente);

            if (historial != null)
            {
                return historial;
            }

            historial =
                new HistorialMedico
                {
                    IdPaciente =
                        idPaciente,

                    FechaCreacion =
                        DateTime.Now
                };

            _context.HistorialesMedicos.Add(
                historial);

            await _context.SaveChangesAsync();

            return historial;
        }
    }
}
