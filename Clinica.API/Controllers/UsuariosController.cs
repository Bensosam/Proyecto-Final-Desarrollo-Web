using Clinica.API.Data;
using Clinica.API.DTOs;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public UsuariosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "UsuariosConsultar")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Empleado)
                .Include(u => u.Rol)
                .OrderBy(u => u.UsuarioLogin)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.IdEmpleado,
                    Empleado = u.Empleado != null
                        ? u.Empleado.Nombres + " " + u.Empleado.Apellidos
                        : null,
                    u.IdRol,
                    Rol = u.Rol != null
                        ? u.Rol.Nombre
                        : null,
                    u.UsuarioLogin,
                    u.Estado
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "UsuariosConsultar")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Empleado)
                .Include(u => u.Rol)
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.IdEmpleado,
                    Empleado = u.Empleado != null
                        ? u.Empleado.Nombres + " " + u.Empleado.Apellidos
                        : null,
                    u.IdRol,
                    Rol = u.Rol != null
                        ? u.Rol.Nombre
                        : null,
                    u.UsuarioLogin,
                    u.Estado
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [HttpPost]
        [Authorize(Policy = "UsuariosCrear")]
        public async Task<IActionResult> CrearUsuario(
            CrearUsuarioDto request)
        {
            if (string.IsNullOrWhiteSpace(request.UsuarioLogin))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de usuario es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password) ||
                request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    mensaje = "La contraseña debe tener al menos 8 caracteres."
                });
            }

            var empleadoExiste = await _context.Empleados
                .AnyAsync(e =>
                    e.IdEmpleado == request.IdEmpleado &&
                    e.Estado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El empleado no existe o está inactivo."
                });
            }

            var rolExiste = await _context.Roles
                .AnyAsync(r =>
                    r.IdRol == request.IdRol &&
                    r.Estado);

            if (!rolExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El rol no existe o está inactivo."
                });
            }

            var loginExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.UsuarioLogin == request.UsuarioLogin);

            if (loginExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de usuario ya está registrado."
                });
            }

            var usuario = new Usuario
            {
                IdEmpleado = request.IdEmpleado,
                IdRol = request.IdRol,
                UsuarioLogin = request.UsuarioLogin.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    request.Password),
                Estado = request.Estado
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUsuario),
                new { id = usuario.IdUsuario },
                new
                {
                    usuario.IdUsuario,
                    usuario.IdEmpleado,
                    usuario.IdRol,
                    usuario.UsuarioLogin,
                    usuario.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "UsuariosModificar")]
        public async Task<IActionResult> ModificarUsuario(
            int id,
            ModificarUsuarioDto request)
        {
            if (id != request.IdUsuario)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del usuario."
                });
            }

            var usuario = await _context.Usuarios
                .FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.UsuarioLogin))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de usuario es obligatorio."
                });
            }

            var empleadoExiste = await _context.Empleados
                .AnyAsync(e =>
                    e.IdEmpleado == request.IdEmpleado &&
                    e.Estado);

            if (!empleadoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El empleado no existe o está inactivo."
                });
            }

            var rolExiste = await _context.Roles
                .AnyAsync(r =>
                    r.IdRol == request.IdRol &&
                    r.Estado);

            if (!rolExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El rol no existe o está inactivo."
                });
            }

            var loginExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.UsuarioLogin == request.UsuarioLogin &&
                    u.IdUsuario != id);

            if (loginExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de usuario ya está registrado."
                });
            }

            var idUsuarioActualTexto = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(
                    idUsuarioActualTexto,
                    out int idUsuarioActual) &&
                idUsuarioActual == id &&
                !request.Estado)
            {
                return BadRequest(new
                {
                    mensaje = "No puede desactivar su propio usuario."
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (request.Password.Length < 8)
                {
                    return BadRequest(new
                    {
                        mensaje = "La contraseña debe tener al menos 8 caracteres."
                    });
                }

                usuario.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password);
            }

            usuario.IdEmpleado = request.IdEmpleado;
            usuario.IdRol = request.IdRol;
            usuario.UsuarioLogin = request.UsuarioLogin.Trim();
            usuario.Estado = request.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                usuario.IdUsuario,
                usuario.IdEmpleado,
                usuario.IdRol,
                usuario.UsuarioLogin,
                usuario.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "UsuariosEliminar")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var idUsuarioActualTexto = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(
                    idUsuarioActualTexto,
                    out int idUsuarioActual) &&
                idUsuarioActual == id)
            {
                return BadRequest(new
                {
                    mensaje = "No puede eliminar su propio usuario."
                });
            }

            var tieneFacturas = await _context.Facturas
                .AnyAsync(f => f.IdUsuario == id);

            if (tieneFacturas)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar porque el usuario tiene facturas registradas."
                });
            }

            var tieneMovimientos = await _context.MovimientosInventario
                .AnyAsync(m => m.IdUsuario == id);

            if (tieneMovimientos)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar porque el usuario tiene movimientos de inventario."
                });
            }

            var tieneVentas = await _context.VentasFarmacia
                .AnyAsync(v => v.IdUsuario == id);

            if (tieneVentas)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar porque el usuario tiene ventas de farmacia."
                });
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}