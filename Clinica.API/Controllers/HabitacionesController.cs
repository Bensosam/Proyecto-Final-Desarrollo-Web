using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabitacionesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public HabitacionesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "HabitacionesConsultar")]
        public async Task<IActionResult> GetHabitaciones()
        {
            var habitaciones = await _context.Habitaciones
                .Include(h => h.Sucursal)
                .Include(h => h.TipoHabitacion)
                .OrderBy(h => h.Numero)
                .Select(h => new
                {
                    h.IdHabitacion,
                    h.IdSucursal,
                    Sucursal = h.Sucursal != null
                        ? h.Sucursal.Nombre
                        : null,
                    h.IdTipoHabitacion,
                    TipoHabitacion = h.TipoHabitacion != null
                        ? h.TipoHabitacion.Nombre
                        : null,
                    PrecioPorDia = h.TipoHabitacion != null
                        ? h.TipoHabitacion.PrecioPorDia
                        : 0,
                    h.Numero,
                    h.CantidadCamas,
                    h.Estado
                })
                .ToListAsync();

            return Ok(habitaciones);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "HabitacionesConsultar")]
        public async Task<IActionResult> GetHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones
                .Include(h => h.Sucursal)
                .Include(h => h.TipoHabitacion)
                .Where(h => h.IdHabitacion == id)
                .Select(h => new
                {
                    h.IdHabitacion,
                    h.IdSucursal,
                    Sucursal = h.Sucursal != null
                        ? h.Sucursal.Nombre
                        : null,
                    h.IdTipoHabitacion,
                    TipoHabitacion = h.TipoHabitacion != null
                        ? h.TipoHabitacion.Nombre
                        : null,
                    PrecioPorDia = h.TipoHabitacion != null
                        ? h.TipoHabitacion.PrecioPorDia
                        : 0,
                    h.Numero,
                    h.CantidadCamas,
                    h.Estado
                })
                .FirstOrDefaultAsync();

            if (habitacion == null)
            {
                return NotFound();
            }

            return Ok(habitacion);
        }

        [HttpPost]
        [Authorize(Policy = "HabitacionesCrear")]
        public async Task<IActionResult> CrearHabitacion(Habitacion habitacion)
        {
            var sucursalExiste = await _context.Sucursales
                .AnyAsync(s => s.IdSucursal == habitacion.IdSucursal);

            if (!sucursalExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La sucursal indicada no existe."
                });
            }

            if (habitacion.IdTipoHabitacion == null)
            {
                return BadRequest(new
                {
                    mensaje = "Debe indicar un tipo de habitación."
                });
            }

            var tipoExiste = await _context.TiposHabitacion
                .AnyAsync(t =>
                    t.IdTipoHabitacion == habitacion.IdTipoHabitacion &&
                    t.Estado);

            if (!tipoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El tipo de habitación indicado no existe o está inactivo."
                });
            }

            if (habitacion.CantidadCamas <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad de camas debe ser mayor que cero."
                });
            }

            var estado = habitacion.Estado.Trim();

            if (estado != "Libre" &&
                estado != "Mantenimiento")
            {
                return BadRequest(new
                {
                    mensaje = "Una habitación nueva solo puede estar Libre o en Mantenimiento."
                });
            }

            habitacion.Estado = estado;

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetHabitacion),
                new { id = habitacion.IdHabitacion },
                new
                {
                    habitacion.IdHabitacion,
                    habitacion.IdSucursal,
                    habitacion.IdTipoHabitacion,
                    habitacion.Numero,
                    habitacion.CantidadCamas,
                    habitacion.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "HabitacionesModificar")]
        public async Task<IActionResult> ModificarHabitacion(
            int id,
            Habitacion habitacion)
        {
            if (id != habitacion.IdHabitacion)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la habitación."
                });
            }

            var habitacionExistente = await _context.Habitaciones
                .FindAsync(id);

            if (habitacionExistente == null)
            {
                return NotFound();
            }

            var sucursalExiste = await _context.Sucursales
                .AnyAsync(s => s.IdSucursal == habitacion.IdSucursal);

            if (!sucursalExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La sucursal indicada no existe."
                });
            }

            if (habitacion.IdTipoHabitacion == null)
            {
                return BadRequest(new
                {
                    mensaje = "Debe indicar un tipo de habitación."
                });
            }

            var tipoExiste = await _context.TiposHabitacion
                .AnyAsync(t =>
                    t.IdTipoHabitacion == habitacion.IdTipoHabitacion &&
                    t.Estado);

            if (!tipoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El tipo de habitación indicado no existe o está inactivo."
                });
            }

            if (habitacion.CantidadCamas <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad de camas debe ser mayor que cero."
                });
            }

            var asignacionesActivas = await _context.AsignacionesHabitacion
                .CountAsync(a =>
                    a.IdHabitacion == id &&
                    a.Estado == "Activa");

            if (habitacion.CantidadCamas < asignacionesActivas)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad de camas no puede ser menor que la cantidad de pacientes actualmente internados."
                });
            }

            var estadoSolicitado = habitacion.Estado.Trim();

            if (estadoSolicitado != "Libre" &&
                estadoSolicitado != "Ocupada" &&
                estadoSolicitado != "Mantenimiento")
            {
                return BadRequest(new
                {
                    mensaje = "El estado debe ser Libre, Ocupada o Mantenimiento."
                });
            }

            if (estadoSolicitado == "Mantenimiento" &&
                asignacionesActivas > 0)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede poner en mantenimiento una habitación con pacientes internados."
                });
            }

            habitacionExistente.IdSucursal = habitacion.IdSucursal;
            habitacionExistente.IdTipoHabitacion =
                habitacion.IdTipoHabitacion;
            habitacionExistente.Numero = habitacion.Numero;
            habitacionExistente.CantidadCamas =
                habitacion.CantidadCamas;

            if (estadoSolicitado == "Mantenimiento")
            {
                habitacionExistente.Estado = "Mantenimiento";
            }
            else
            {
                habitacionExistente.Estado =
                    asignacionesActivas > 0
                        ? "Ocupada"
                        : "Libre";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                habitacionExistente.IdHabitacion,
                habitacionExistente.IdSucursal,
                habitacionExistente.IdTipoHabitacion,
                habitacionExistente.Numero,
                habitacionExistente.CantidadCamas,
                habitacionExistente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "HabitacionesEliminar")]
        public async Task<IActionResult> EliminarHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones
                .FindAsync(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            var tieneAsignaciones = await _context.AsignacionesHabitacion
                .AnyAsync(a => a.IdHabitacion == id);

            if (tieneAsignaciones)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar una habitación que tiene historial de asignaciones."
                });
            }

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}