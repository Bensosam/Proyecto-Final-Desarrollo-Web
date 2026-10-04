using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvolucionesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public EvolucionesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "EvolucionesConsultar")]
        public async Task<IActionResult> GetEvoluciones()
        {
            var evoluciones = await _context.Evoluciones
                .Include(e => e.Empleado)
                .OrderByDescending(e => e.Fecha)
                .Select(e => new
                {
                    e.IdEvolucion,
                    e.IdConsulta,
                    e.IdEmpleado,
                    Empleado = e.Empleado != null
                        ? e.Empleado.Nombres + " " + e.Empleado.Apellidos
                        : null,
                    e.Fecha,
                    e.Descripcion
                })
                .ToListAsync();

            return Ok(evoluciones);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "EvolucionesConsultar")]
        public async Task<IActionResult> GetEvolucion(int id)
        {
            var evolucion = await _context.Evoluciones
                .Include(e => e.Empleado)
                .Where(e => e.IdEvolucion == id)
                .Select(e => new
                {
                    e.IdEvolucion,
                    e.IdConsulta,
                    e.IdEmpleado,
                    Empleado = e.Empleado != null
                        ? e.Empleado.Nombres + " " + e.Empleado.Apellidos
                        : null,
                    e.Fecha,
                    e.Descripcion
                })
                .FirstOrDefaultAsync();

            if (evolucion == null)
            {
                return NotFound();
            }

            return Ok(evolucion);
        }

        [HttpPost]
        [Authorize(Policy = "EvolucionesCrear")]
        public async Task<IActionResult> CrearEvolucion(Evolucion evolucion)
        {
            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == evolucion.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            var empleadoExiste = await _context.Empleados
                .AnyAsync(e => e.IdEmpleado == evolucion.IdEmpleado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El empleado indicado no existe."
                });
            }

            if (evolucion.Fecha == default)
            {
                evolucion.Fecha = DateTime.Now;
            }

            _context.Evoluciones.Add(evolucion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvolucion),
                new { id = evolucion.IdEvolucion },
                new
                {
                    evolucion.IdEvolucion,
                    evolucion.IdConsulta,
                    evolucion.IdEmpleado,
                    evolucion.Fecha,
                    evolucion.Descripcion
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "EvolucionesModificar")]
        public async Task<IActionResult> ModificarEvolucion(
            int id,
            Evolucion evolucion)
        {
            if (id != evolucion.IdEvolucion)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la evolución."
                });
            }

            var evolucionExistente = await _context.Evoluciones
                .FindAsync(id);

            if (evolucionExistente == null)
            {
                return NotFound();
            }

            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.IdConsulta == evolucion.IdConsulta);

            if (!consultaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La consulta indicada no existe."
                });
            }

            var empleadoExiste = await _context.Empleados
                .AnyAsync(e => e.IdEmpleado == evolucion.IdEmpleado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El empleado indicado no existe."
                });
            }

            evolucionExistente.IdConsulta = evolucion.IdConsulta;
            evolucionExistente.IdEmpleado = evolucion.IdEmpleado;
            evolucionExistente.Fecha = evolucion.Fecha;
            evolucionExistente.Descripcion = evolucion.Descripcion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                evolucionExistente.IdEvolucion,
                evolucionExistente.IdConsulta,
                evolucionExistente.IdEmpleado,
                evolucionExistente.Fecha,
                evolucionExistente.Descripcion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "EvolucionesEliminar")]
        public async Task<IActionResult> EliminarEvolucion(int id)
        {
            var evolucion = await _context.Evoluciones
                .FindAsync(id);

            if (evolucion == null)
            {
                return NotFound();
            }

            _context.Evoluciones.Remove(evolucion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}