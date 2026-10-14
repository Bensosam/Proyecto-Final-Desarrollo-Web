using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolPermisosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public RolPermisosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetPermisos()
        {
            var permisos = await _context.RolPermisos
                .Include(rp => rp.Rol)
                .Include(rp => rp.Modulo)
                .Include(rp => rp.Operacion)
                .OrderBy(rp => rp.Rol!.Nombre)
                .ThenBy(rp => rp.Modulo!.Nombre)
                .ThenBy(rp => rp.Operacion!.Nombre)
                .Select(rp => new
                {
                    rp.IdRolPermiso,
                    rp.IdRol,
                    Rol = rp.Rol != null
                        ? rp.Rol.Nombre
                        : null,
                    rp.IdModulo,
                    Modulo = rp.Modulo != null
                        ? rp.Modulo.Nombre
                        : null,
                    rp.IdOperacion,
                    Operacion = rp.Operacion != null
                        ? rp.Operacion.Nombre
                        : null
                })
                .ToListAsync();

            return Ok(permisos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetPermiso(int id)
        {
            var permiso = await _context.RolPermisos
                .Include(rp => rp.Rol)
                .Include(rp => rp.Modulo)
                .Include(rp => rp.Operacion)
                .Where(rp => rp.IdRolPermiso == id)
                .Select(rp => new
                {
                    rp.IdRolPermiso,
                    rp.IdRol,
                    Rol = rp.Rol != null
                        ? rp.Rol.Nombre
                        : null,
                    rp.IdModulo,
                    Modulo = rp.Modulo != null
                        ? rp.Modulo.Nombre
                        : null,
                    rp.IdOperacion,
                    Operacion = rp.Operacion != null
                        ? rp.Operacion.Nombre
                        : null
                })
                .FirstOrDefaultAsync();

            if (permiso == null)
            {
                return NotFound();
            }

            return Ok(permiso);
        }

        [HttpGet("rol/{idRol}")]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetPorRol(int idRol)
        {
            var rolExiste = await _context.Roles
                .AnyAsync(r => r.IdRol == idRol);

            if (!rolExiste)
            {
                return NotFound(new
                {
                    mensaje = "El rol indicado no existe."
                });
            }

            var permisos = await _context.RolPermisos
                .Include(rp => rp.Modulo)
                .Include(rp => rp.Operacion)
                .Where(rp => rp.IdRol == idRol)
                .OrderBy(rp => rp.Modulo!.Nombre)
                .ThenBy(rp => rp.Operacion!.Nombre)
                .Select(rp => new
                {
                    rp.IdRolPermiso,
                    rp.IdRol,
                    rp.IdModulo,
                    Modulo = rp.Modulo != null
                        ? rp.Modulo.Nombre
                        : null,
                    rp.IdOperacion,
                    Operacion = rp.Operacion != null
                        ? rp.Operacion.Nombre
                        : null
                })
                .ToListAsync();

            return Ok(permisos);
        }

        [HttpPost]
        [Authorize(Policy = "RolPermisosCrear")]
        public async Task<IActionResult> CrearPermiso(
            RolPermiso permiso)
        {
            var rolExiste = await _context.Roles
                .AnyAsync(r =>
                    r.IdRol == permiso.IdRol &&
                    r.Estado);

            if (!rolExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El rol no existe o está inactivo."
                });
            }

            var moduloExiste = await _context.Modulos
                .AnyAsync(m =>
                    m.IdModulo == permiso.IdModulo &&
                    m.Estado);

            if (!moduloExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El módulo no existe o está inactivo."
                });
            }

            var operacionExiste = await _context.Operaciones
                .AnyAsync(o =>
                    o.IdOperacion == permiso.IdOperacion);

            if (!operacionExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La operación indicada no existe."
                });
            }

            var duplicado = await _context.RolPermisos
                .AnyAsync(rp =>
                    rp.IdRol == permiso.IdRol &&
                    rp.IdModulo == permiso.IdModulo &&
                    rp.IdOperacion == permiso.IdOperacion);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje = "El permiso ya está asignado al rol."
                });
            }

            _context.RolPermisos.Add(permiso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPermiso),
                new { id = permiso.IdRolPermiso },
                new
                {
                    permiso.IdRolPermiso,
                    permiso.IdRol,
                    permiso.IdModulo,
                    permiso.IdOperacion
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RolPermisosModificar")]
        public async Task<IActionResult> ModificarPermiso(
            int id,
            RolPermiso permiso)
        {
            if (id != permiso.IdRolPermiso)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del permiso."
                });
            }

            var existente = await _context.RolPermisos
                .Include(rp => rp.Rol)
                .FirstOrDefaultAsync(rp =>
                    rp.IdRolPermiso == id);

            if (existente == null)
            {
                return NotFound();
            }

            if (existente.Rol?.Nombre == "Administrador")
            {
                return BadRequest(new
                {
                    mensaje = "No se pueden modificar los permisos del rol Administrador desde este endpoint."
                });
            }

            var rolExiste = await _context.Roles
                .AnyAsync(r =>
                    r.IdRol == permiso.IdRol &&
                    r.Estado);

            if (!rolExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El rol no existe o está inactivo."
                });
            }

            var moduloExiste = await _context.Modulos
                .AnyAsync(m =>
                    m.IdModulo == permiso.IdModulo &&
                    m.Estado);

            if (!moduloExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El módulo no existe o está inactivo."
                });
            }

            var operacionExiste = await _context.Operaciones
                .AnyAsync(o =>
                    o.IdOperacion == permiso.IdOperacion);

            if (!operacionExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La operación indicada no existe."
                });
            }

            var duplicado = await _context.RolPermisos
                .AnyAsync(rp =>
                    rp.IdRol == permiso.IdRol &&
                    rp.IdModulo == permiso.IdModulo &&
                    rp.IdOperacion == permiso.IdOperacion &&
                    rp.IdRolPermiso != id);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe esa combinación de rol, módulo y operación."
                });
            }

            existente.IdRol = permiso.IdRol;
            existente.IdModulo = permiso.IdModulo;
            existente.IdOperacion = permiso.IdOperacion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdRolPermiso,
                existente.IdRol,
                existente.IdModulo,
                existente.IdOperacion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RolPermisosEliminar")]
        public async Task<IActionResult> EliminarPermiso(int id)
        {
            var permiso = await _context.RolPermisos
                .Include(rp => rp.Rol)
                .FirstOrDefaultAsync(rp =>
                    rp.IdRolPermiso == id);

            if (permiso == null)
            {
                return NotFound();
            }

            if (permiso.Rol?.Nombre == "Administrador")
            {
                return BadRequest(new
                {
                    mensaje = "No se pueden eliminar permisos del rol Administrador."
                });
            }

            _context.RolPermisos.Remove(permiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}