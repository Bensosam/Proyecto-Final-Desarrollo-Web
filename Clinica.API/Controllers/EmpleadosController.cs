using Clinica.API.Authorization;
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

        public EmpleadosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "EmpleadosConsultar")]
        public async Task<IActionResult> GetEmpleados()
        {
            var empleados = await _context.Empleados
    .Include(e => e.Sucursal)
    .Include(e => e.Especialidad)
    .Include(e => e.Turno)
    .Select(e => new
    {
        e.IdEmpleado,
        e.IdSucursal,
        Sucursal = e.Sucursal!.Nombre,
        e.IdTurno,
        Turno = e.Turno!.Nombre,
        e.IdEspecialidad,
        Especialidad = e.Especialidad != null
            ? e.Especialidad.Nombre
            : null,
        e.Nombres,
        e.Apellidos,
        e.DPI,
        e.Telefono,
        e.Correo,
        e.Estado
    })
    .ToListAsync();

return Ok(empleados);
        }

        [HttpPost]
        [Authorize(Policy = "EmpleadosCrear")]
        public async Task<IActionResult> CrearEmpleado(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEmpleados),
                new { id = empleado.IdEmpleado },
                empleado);
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
                    mensaje = "El ID de la URL no coincide con el ID del empleado."
                });
            }

            var empleadoExistente = await _context.Empleados
                .FindAsync(id);

            if (empleadoExistente == null)
            {
                return NotFound();
            }

            empleadoExistente.IdSucursal = empleado.IdSucursal;
            empleadoExistente.IdTurno = empleado.IdTurno;
            empleadoExistente.IdEspecialidad = empleado.IdEspecialidad;
            empleadoExistente.Nombres = empleado.Nombres;
            empleadoExistente.Apellidos = empleado.Apellidos;
            empleadoExistente.DPI = empleado.DPI;
            empleadoExistente.Telefono = empleado.Telefono;
            empleadoExistente.Correo = empleado.Correo;
            empleadoExistente.Estado = empleado.Estado;

            await _context.SaveChangesAsync();

            return Ok(empleadoExistente);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "EmpleadosEliminar")]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);

            if (empleado == null)
            {
                return NotFound();
            }

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
