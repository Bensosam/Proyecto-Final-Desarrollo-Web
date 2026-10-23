using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public EmpleadosController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "EmpleadosConsultar")]
        public async Task<IActionResult> GetEmpleados()
        {
            var empleados =
                await _context.Empleados
                    .Include(e => e.Sucursal)
                    .Include(e => e.Especialidad)
                    .Include(e => e.Turno)
                    .OrderBy(e => e.Apellidos)
                    .ThenBy(e => e.Nombres)
                    .Select(e => new
                    {
                        e.IdEmpleado,

                        e.IdSucursal,
                        Sucursal =
                            e.Sucursal != null
                                ? e.Sucursal.Nombre
                                : null,

                        e.IdEspecialidad,
                        Especialidad =
                            e.Especialidad != null
                                ? e.Especialidad.Nombre
                                : null,

                        e.IdTurno,
                        Turno =
                            e.Turno != null
                                ? e.Turno.Nombre
                                : null,

                        e.Nombres,
                        e.Apellidos,
                        e.Puesto,
                        e.Telefono,
                        e.Correo,
                        e.Direccion,
                        e.Estado
                    })
                    .ToListAsync();

            return Ok(empleados);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "EmpleadosConsultar")]
        public async Task<IActionResult> GetEmpleado(
            int id)
        {
            var empleado =
                await _context.Empleados
                    .Where(e =>
                        e.IdEmpleado == id)
                    .Select(e => new
                    {
                        e.IdEmpleado,

                        e.IdSucursal,
                        Sucursal =
                            e.Sucursal != null
                                ? e.Sucursal.Nombre
                                : null,

                        e.IdEspecialidad,
                        Especialidad =
                            e.Especialidad != null
                                ? e.Especialidad.Nombre
                                : null,

                        e.IdTurno,
                        Turno =
                            e.Turno != null
                                ? e.Turno.Nombre
                                : null,

                        e.Nombres,
                        e.Apellidos,
                        e.Puesto,
                        e.Telefono,
                        e.Correo,
                        e.Direccion,
                        e.Estado
                    })
                    .FirstOrDefaultAsync();

            if (empleado == null)
            {
                return NotFound();
            }

            return Ok(empleado);
        }

        [HttpPost]
        [Authorize(Policy = "EmpleadosCrear")]
        public async Task<IActionResult> CrearEmpleado(
            Empleado empleado)
        {
            var sucursalExiste =
                await _context.Sucursales.AnyAsync(
                    s => s.IdSucursal ==
                         empleado.IdSucursal);

            if (!sucursalExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La sucursal indicada no existe."
                });
            }

            var turnoExiste =
                await _context.Turnos.AnyAsync(
                    t => t.IdTurno ==
                         empleado.IdTurno);

            if (!turnoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El turno indicado no existe."
                });
            }

            if (empleado.IdEspecialidad.HasValue)
            {
                var especialidadExiste =
                    await _context.Especialidades.AnyAsync(
                        e =>
                            e.IdEspecialidad ==
                            empleado.IdEspecialidad.Value);

                if (!especialidadExiste)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La especialidad indicada no existe."
                    });
                }
            }

            empleado.DPI ??= string.Empty;
            empleado.Puesto ??= string.Empty;
            empleado.Telefono ??= string.Empty;
            empleado.Correo ??= string.Empty;
            empleado.Direccion ??= string.Empty;

            _context.Empleados.Add(
                empleado);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEmpleado),
                new
                {
                    id = empleado.IdEmpleado
                },
                new
                {
                    empleado.IdEmpleado,
                    empleado.IdSucursal,
                    empleado.IdEspecialidad,
                    empleado.IdTurno,
                    empleado.Nombres,
                    empleado.Apellidos,
                    empleado.Puesto,
                    empleado.Telefono,
                    empleado.Correo,
                    empleado.Direccion,
                    empleado.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "EmpleadosModificar")]
        public async Task<IActionResult> ModificarEmpleado(
            int id,
            Empleado empleado)
        {
            if (id != empleado.IdEmpleado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del empleado."
                });
            }

            var existente =
                await _context.Empleados
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var sucursalExiste =
                await _context.Sucursales.AnyAsync(
                    s => s.IdSucursal ==
                         empleado.IdSucursal);

            if (!sucursalExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La sucursal indicada no existe."
                });
            }

            var turnoExiste =
                await _context.Turnos.AnyAsync(
                    t => t.IdTurno ==
                         empleado.IdTurno);

            if (!turnoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El turno indicado no existe."
                });
            }

            if (empleado.IdEspecialidad.HasValue)
            {
                var especialidadExiste =
                    await _context.Especialidades.AnyAsync(
                        e =>
                            e.IdEspecialidad ==
                            empleado.IdEspecialidad.Value);

                if (!especialidadExiste)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La especialidad indicada no existe."
                    });
                }
            }

            existente.IdSucursal =
                empleado.IdSucursal;

            existente.IdTurno =
                empleado.IdTurno;

            existente.IdEspecialidad =
                empleado.IdEspecialidad;

            existente.Nombres =
                empleado.Nombres;

            existente.Apellidos =
                empleado.Apellidos;

            // DPI no se toca porque el formulario actual
            // no administra ese dato.

            existente.Puesto =
                empleado.Puesto ?? string.Empty;

            existente.Telefono =
                empleado.Telefono ?? string.Empty;

            existente.Correo =
                empleado.Correo ?? string.Empty;

            existente.Direccion =
                empleado.Direccion ?? string.Empty;

            existente.Estado =
                empleado.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdEmpleado,
                existente.IdSucursal,
                existente.IdEspecialidad,
                existente.IdTurno,
                existente.Nombres,
                existente.Apellidos,
                existente.Puesto,
                existente.Telefono,
                existente.Correo,
                existente.Direccion,
                existente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "EmpleadosEliminar")]
        public async Task<IActionResult> EliminarEmpleado(
            int id)
        {
            var empleado =
                await _context.Empleados
                    .FindAsync(id);

            if (empleado == null)
            {
                return NotFound();
            }

            _context.Empleados.Remove(
                empleado);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
