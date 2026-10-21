using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TratamientosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public TratamientosController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "TratamientosConsultar")]
        public async Task<IActionResult> GetTratamientos()
        {
            var tratamientos =
                await _context.Tratamientos
                    .OrderByDescending(t => t.FechaInicio)
                    .Select(t => new
                    {
                        t.IdTratamiento,
                        t.IdConsulta,

                        IdPaciente =
                            t.Consulta != null
                                ? t.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            t.Consulta != null &&
                            t.Consulta.Paciente != null
                                ? t.Consulta.Paciente.Nombres +
                                  " " +
                                  t.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            t.Consulta != null
                                ? t.Consulta.NumeroConsultaPaciente
                                : 0,

                        t.Descripcion,
                        t.Indicaciones,
                        t.FechaInicio,
                        t.FechaFin
                    })
                    .ToListAsync();

            return Ok(tratamientos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "TratamientosConsultar")]
        public async Task<IActionResult> GetTratamiento(
            int id)
        {
            var tratamiento =
                await _context.Tratamientos
                    .Where(t =>
                        t.IdTratamiento == id)
                    .Select(t => new
                    {
                        t.IdTratamiento,
                        t.IdConsulta,

                        IdPaciente =
                            t.Consulta != null
                                ? t.Consulta.IdPaciente
                                : 0,

                        Paciente =
                            t.Consulta != null &&
                            t.Consulta.Paciente != null
                                ? t.Consulta.Paciente.Nombres +
                                  " " +
                                  t.Consulta.Paciente.Apellidos
                                : null,

                        NumeroConsultaPaciente =
                            t.Consulta != null
                                ? t.Consulta.NumeroConsultaPaciente
                                : 0,

                        t.Descripcion,
                        t.Indicaciones,
                        t.FechaInicio,
                        t.FechaFin
                    })
                    .FirstOrDefaultAsync();

            if (tratamiento == null)
            {
                return NotFound();
            }

            return Ok(tratamiento);
        }

        [HttpPost]
        [Authorize(Policy = "TratamientosCrear")]
        public async Task<IActionResult> CrearTratamiento(
            Tratamiento tratamiento)
        {
            var consultaExiste =
                await _context.Consultas
                    .AnyAsync(c =>
                        c.IdConsulta ==
                        tratamiento.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La consulta indicada no existe."
                });
            }

            if (tratamiento.FechaInicio == default)
            {
                tratamiento.FechaInicio =
                    DateTime.Now;
            }

            _context.Tratamientos.Add(
                tratamiento);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTratamiento),
                new { id = tratamiento.IdTratamiento },
                tratamiento);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "TratamientosModificar")]
        public async Task<IActionResult> ModificarTratamiento(
            int id,
            Tratamiento tratamiento)
        {
            if (id != tratamiento.IdTratamiento)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del tratamiento."
                });
            }

            var existente =
                await _context.Tratamientos
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var consultaExiste =
                await _context.Consultas
                    .AnyAsync(c =>
                        c.IdConsulta ==
                        tratamiento.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La consulta indicada no existe."
                });
            }

            existente.IdConsulta =
                tratamiento.IdConsulta;

            existente.Descripcion =
                tratamiento.Descripcion;

            existente.Indicaciones =
                tratamiento.Indicaciones;

            existente.FechaInicio =
                tratamiento.FechaInicio;

            existente.FechaFin =
                tratamiento.FechaFin;

            await _context.SaveChangesAsync();

            return Ok(existente);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "TratamientosEliminar")]
        public async Task<IActionResult> EliminarTratamiento(
            int id)
        {
            var tratamiento =
                await _context.Tratamientos
                    .FindAsync(id);

            if (tratamiento == null)
            {
                return NotFound();
            }

            _context.Tratamientos.Remove(
                tratamiento);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}