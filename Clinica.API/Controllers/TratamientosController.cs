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

        public TratamientosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "TratamientosConsultar")]
        public async Task<IActionResult> GetTratamientos()
        {
            var tratamientos = await _context.Tratamientos
                .OrderByDescending(t => t.IdTratamiento)
                .Select(t => new
                {
                    t.IdTratamiento,
                    t.IdConsulta,
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
        public async Task<IActionResult> GetTratamiento(int id)
        {
            var tratamiento = await _context.Tratamientos
                .Where(t => t.IdTratamiento == id)
                .Select(t => new
                {
                    t.IdTratamiento,
                    t.IdConsulta,
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
        public async Task<IActionResult> CrearTratamiento(Tratamiento tratamiento)
        {
            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == tratamiento.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            _context.Tratamientos.Add(tratamiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTratamiento),
                new { id = tratamiento.IdTratamiento },
                new
                {
                    tratamiento.IdTratamiento,
                    tratamiento.IdConsulta,
                    tratamiento.Descripcion,
                    tratamiento.Indicaciones,
                    tratamiento.FechaInicio,
                    tratamiento.FechaFin
                });
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
                    mensaje = "El ID de la URL no coincide con el ID del tratamiento."
                });
            }

            var tratamientoExistente = await _context.Tratamientos
                .FindAsync(id);

            if (tratamientoExistente == null)
            {
                return NotFound();
            }

            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == tratamiento.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            tratamientoExistente.IdConsulta = tratamiento.IdConsulta;
            tratamientoExistente.Descripcion = tratamiento.Descripcion;
            tratamientoExistente.Indicaciones = tratamiento.Indicaciones;
            tratamientoExistente.FechaInicio = tratamiento.FechaInicio;
            tratamientoExistente.FechaFin = tratamiento.FechaFin;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                tratamientoExistente.IdTratamiento,
                tratamientoExistente.IdConsulta,
                tratamientoExistente.Descripcion,
                tratamientoExistente.Indicaciones,
                tratamientoExistente.FechaInicio,
                tratamientoExistente.FechaFin
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "TratamientosEliminar")]
        public async Task<IActionResult> EliminarTratamiento(int id)
        {
            var tratamiento = await _context.Tratamientos
                .FindAsync(id);

            if (tratamiento == null)
            {
                return NotFound();
            }

            _context.Tratamientos.Remove(tratamiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}