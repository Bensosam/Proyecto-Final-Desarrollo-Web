using System.Security.Claims;
using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModulosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public ModulosController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // MODULOS QUE PUEDE VER EL USUARIO ACTUAL
        //
        // Este endpoint NO requiere ModulosConsultar.
        // Cualquier usuario autenticado puede consultar solamente
        // los modulos para los que su rol tenga permiso Consultar.
        // =========================================================

        [HttpGet("mis-modulos")]
        [Authorize]
        public async Task<IActionResult> GetMisModulos()
        {
            var idUsuarioTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    idUsuarioTexto,
                    out var idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje =
                        "No fue posible identificar al usuario."
                });
            }

            var usuario =
                await _context.Usuarios
                    .AsNoTracking()
                    .Where(u =>
                        u.IdUsuario == idUsuario &&
                        u.Estado)
                    .Select(u => new
                    {
                        u.IdUsuario,
                        u.IdRol
                    })
                    .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "El usuario no existe o está inactivo."
                });
            }

            var permisos =
    await _context.RolPermisos
        .AsNoTracking()
        .Where(rp =>
            rp.IdRol == usuario.IdRol &&

            rp.Modulo != null &&
            rp.Modulo.Estado &&

            rp.Operacion != null)
        .Select(rp => new
        {
            rp.IdModulo,

            Nombre =
                rp.Modulo!.Nombre,

            Descripcion =
                rp.Modulo.Descripcion,

            Operacion =
                rp.Operacion!.Nombre
        })
        .ToListAsync();

var modulos =
    permisos
        .GroupBy(p => new
        {
            p.IdModulo,
            p.Nombre,
            p.Descripcion
        })
        .Where(g =>
            g.Any(p =>
                p.Operacion == "Consultar"))
        .Select(g => new
        {
            g.Key.IdModulo,
            g.Key.Nombre,
            g.Key.Descripcion,

            Operaciones =
                g.Select(p => p.Operacion)
                    .Distinct()
                    .OrderBy(o => o)
                    .ToList()
        })
        .OrderBy(m => m.Nombre)
        .ToList();

return Ok(modulos);
        }

        // =========================================================
        // CONSULTAR TODOS LOS MODULOS
        // ADMINISTRACION
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "ModulosConsultar")]
        public async Task<IActionResult> GetModulos()
        {
            var modulos =
                await _context.Modulos
                    .OrderBy(m =>
                        m.Nombre)
                    .Select(m => new
                    {
                        m.IdModulo,
                        m.Nombre,
                        m.Descripcion,
                        m.Estado,

                        CantidadPermisos =
                            m.RolPermisos.Count
                    })
                    .ToListAsync();

            return Ok(modulos);
        }

        // =========================================================
        // CONSULTAR MODULO
        // =========================================================

        [HttpGet("{id}")]
        [Authorize(Policy = "ModulosConsultar")]
        public async Task<IActionResult> GetModulo(
            int id)
        {
            var modulo =
                await _context.Modulos
                    .Where(m =>
                        m.IdModulo == id)
                    .Select(m => new
                    {
                        m.IdModulo,
                        m.Nombre,
                        m.Descripcion,
                        m.Estado,

                        CantidadPermisos =
                            m.RolPermisos.Count
                    })
                    .FirstOrDefaultAsync();

            if (modulo == null)
            {
                return NotFound();
            }

            return Ok(modulo);
        }

        // =========================================================
        // CREAR MODULO
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "ModulosCrear")]
        public async Task<IActionResult> CrearModulo(
            Modulo modulo)
        {
            if (string.IsNullOrWhiteSpace(
                    modulo.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre del módulo es obligatorio."
                });
            }

            var nombre =
                modulo.Nombre.Trim();

            var existe =
                await _context.Modulos
                    .AnyAsync(m =>
                        m.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe un módulo con ese nombre."
                });
            }

            modulo.Nombre =
                nombre;

            modulo.Descripcion =
                modulo.Descripcion?.Trim() ??
                string.Empty;

            _context.Modulos.Add(
                modulo);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetModulo),
                new
                {
                    id =
                        modulo.IdModulo
                },
                new
                {
                    modulo.IdModulo,
                    modulo.Nombre,
                    modulo.Descripcion,
                    modulo.Estado
                });
        }

        // =========================================================
        // MODIFICAR MODULO
        // =========================================================

        [HttpPut("{id}")]
        [Authorize(Policy = "ModulosModificar")]
        public async Task<IActionResult> ModificarModulo(
            int id,
            Modulo modulo)
        {
            if (id != modulo.IdModulo)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del módulo."
                });
            }

            var existente =
                await _context.Modulos
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(
                    modulo.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre del módulo es obligatorio."
                });
            }

            var nombre =
                modulo.Nombre.Trim();

            var nombreExiste =
                await _context.Modulos
                    .AnyAsync(m =>
                        m.Nombre == nombre &&
                        m.IdModulo != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otro módulo con ese nombre."
                });
            }

            var tienePermisos =
                await _context.RolPermisos
                    .AnyAsync(rp =>
                        rp.IdModulo == id);

            if (tienePermisos &&
                existente.Nombre != nombre)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede cambiar el nombre de un módulo que ya tiene permisos asignados."
                });
            }

            existente.Nombre =
                nombre;

            existente.Descripcion =
                modulo.Descripcion?.Trim() ??
                string.Empty;

            existente.Estado =
                modulo.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdModulo,
                existente.Nombre,
                existente.Descripcion,
                existente.Estado
            });
        }

        // =========================================================
        // ELIMINAR MODULO
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize(Policy = "ModulosEliminar")]
        public async Task<IActionResult> EliminarModulo(
            int id)
        {
            var modulo =
                await _context.Modulos
                    .FindAsync(id);

            if (modulo == null)
            {
                return NotFound();
            }

            var tienePermisos =
                await _context.RolPermisos
                    .AnyAsync(rp =>
                        rp.IdModulo == id);

            if (tienePermisos)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar el módulo porque tiene permisos asignados."
                });
            }

            _context.Modulos.Remove(
                modulo);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}