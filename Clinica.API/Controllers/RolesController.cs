using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public RolesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "RolesConsultar")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    r.IdRol,
                    r.Nombre,
                    r.Descripcion,
                    r.Estado,
                    CantidadUsuarios = r.Usuarios.Count,
                    CantidadPermisos = r.RolPermisos.Count
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "RolesConsultar")]
        public async Task<IActionResult> GetRol(int id)
        {
            var rol = await _context.Roles
                .Where(r => r.IdRol == id)
                .Select(r => new
                {
                    r.IdRol,
                    r.Nombre,
                    r.Descripcion,
                    r.Estado,
                    CantidadUsuarios = r.Usuarios.Count,
                    CantidadPermisos = r.RolPermisos.Count
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                return NotFound();
            }

            return Ok(rol);
        }

        [HttpPost]
        [Authorize(Policy = "RolesCrear")]
        public async Task<IActionResult> CrearRol(Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del rol es obligatorio."
                });
            }

            var nombre = rol.Nombre.Trim();

            var existe = await _context.Roles
                .AnyAsync(r => r.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe un rol con ese nombre."
                });
            }

            rol.Nombre = nombre;
            rol.Descripcion = rol.Descripcion?.Trim() ?? string.Empty;

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRol),
                new { id = rol.IdRol },
                new
                {
                    rol.IdRol,
                    rol.Nombre,
                    rol.Descripcion,
                    rol.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RolesModificar")]
        public async Task<IActionResult> ModificarRol(
            int id,
            Rol rol)
        {
            if (id != rol.IdRol)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del rol."
                });
            }

            var existente = await _context.Roles
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del rol es obligatorio."
                });
            }

            var nombre = rol.Nombre.Trim();

            var nombreExiste = await _context.Roles
                .AnyAsync(r =>
                    r.Nombre == nombre &&
                    r.IdRol != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otro rol con ese nombre."
                });
            }

            if (existente.Nombre == "Administrador" &&
                nombre != "Administrador")
            {
                return BadRequest(new
                {
                    mensaje = "No se puede cambiar el nombre del rol Administrador."
                });
            }

            if (existente.Nombre == "Administrador" &&
                !rol.Estado)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede desactivar el rol Administrador."
                });
            }

            existente.Nombre = nombre;
            existente.Descripcion =
                rol.Descripcion?.Trim() ?? string.Empty;
            existente.Estado = rol.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdRol,
                existente.Nombre,
                existente.Descripcion,
                existente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RolesEliminar")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            var rol = await _context.Roles
                .FindAsync(id);

            if (rol == null)
            {
                return NotFound();
            }

            if (rol.Nombre == "Administrador")
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar el rol Administrador."
                });
            }

            var tieneUsuarios = await _context.Usuarios
                .AnyAsync(u => u.IdRol == id);

            if (tieneUsuarios)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar el rol porque tiene usuarios asociados."
                });
            }

            var tienePermisos = await _context.RolPermisos
                .AnyAsync(rp => rp.IdRol == id);

            if (tienePermisos)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar el rol porque tiene permisos asociados."
                });
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}