using Clinica.API.Data;
using Clinica.API.DTOs;
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

        public RolPermisosController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // RESUMEN AGRUPADO POR ROL
        // =========================================================

        [HttpGet("resumen")]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetResumen()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    r.IdRol,
                    r.Nombre,
                    r.Descripcion,
                    r.Estado,
                    CantidadUsuarios = r.Usuarios.Count
                })
                .ToListAsync();

            var permisos = await _context.RolPermisos
                .AsNoTracking()
                .Where(rp =>
                    rp.Modulo != null &&
                    rp.Operacion != null)
                .Select(rp => new
                {
                    rp.IdRolPermiso,
                    rp.IdRol,
                    rp.IdModulo,
                    Modulo = rp.Modulo!.Nombre,
                    rp.IdOperacion,
                    Operacion = rp.Operacion!.Nombre
                })
                .ToListAsync();

            var resultado = roles
                .Select(rol =>
                {
                    var permisosRol = permisos
                        .Where(p =>
                            p.IdRol == rol.IdRol)
                        .ToList();

                    var modulos = permisosRol
                        .GroupBy(p => new
                        {
                            p.IdModulo,
                            p.Modulo
                        })
                        .OrderBy(g =>
                            g.Key.Modulo)
                        .Select(g => new
                        {
                            g.Key.IdModulo,
                            Nombre = g.Key.Modulo,

                            Operaciones = g
                                .OrderBy(p =>
                                    OrdenOperacion(
                                        p.Operacion))
                                .ThenBy(p =>
                                    p.Operacion)
                                .Select(p => new
                                {
                                    p.IdRolPermiso,
                                    p.IdOperacion,
                                    Nombre =
                                        p.Operacion
                                })
                                .ToList()
                        })
                        .ToList();

                    return new
                    {
                        rol.IdRol,
                        rol.Nombre,
                        rol.Descripcion,
                        rol.Estado,
                        rol.CantidadUsuarios,

                        Protegido =
                            rol.Nombre ==
                            "Administrador",

                        CantidadModulos =
                            modulos.Count,

                        CantidadPermisos =
                            permisosRol.Count,

                        Modulos =
                            modulos
                    };
                })
                .ToList();

            return Ok(resultado);
        }

        // =========================================================
        // DATOS PARA EL ADMINISTRADOR DE PERMISOS
        //
        // Se sirven desde este controlador para que la pantalla
        // de permisos no dependa de tener permisos administrativos
        // sobre Roles, Modulos u Operaciones por separado.
        // =========================================================

        [HttpGet("configuracion")]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetConfiguracion()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    r.IdRol,
                    r.Nombre,
                    r.Descripcion,
                    r.Estado,

                    Protegido =
                        r.Nombre ==
                        "Administrador"
                })
                .ToListAsync();

            var modulos = await _context.Modulos
                .AsNoTracking()
                .Where(m => m.Estado)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.IdModulo,
                    m.Nombre,
                    m.Descripcion
                })
                .ToListAsync();

            var operacionesDb =
                await _context.Operaciones
                    .AsNoTracking()
                    .Select(o => new
                    {
                        o.IdOperacion,
                        o.Nombre
                    })
                    .ToListAsync();

            var operaciones = operacionesDb
                .OrderBy(o =>
                    OrdenOperacion(o.Nombre))
                .ThenBy(o =>
                    o.Nombre)
                .ToList();

            return Ok(new
            {
                Roles = roles,
                Modulos = modulos,
                Operaciones = operaciones
            });
        }

        // =========================================================
        // APLICAR TODOS LOS PERMISOS DE UN ROL
        //
        // Reemplaza la configuración completa del rol de forma
        // atómica:
        // - agrega permisos nuevos
        // - conserva los que siguen seleccionados
        // - elimina los que fueron quitados
        // =========================================================

        [HttpPut("rol/{idRol}/aplicar")]
        [Authorize(Policy = "RolPermisosCrear")]
        [Authorize(Policy = "RolPermisosModificar")]
        [Authorize(Policy = "RolPermisosEliminar")]
        public async Task<IActionResult> AplicarPermisosRol(
            int idRol,
            AplicarPermisosRolDto dto)
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.IdRol == idRol);

            if (rol == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El rol indicado no existe."
                });
            }

            if (!rol.Estado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pueden administrar permisos de un rol inactivo."
                });
            }

            if (rol.Nombre == "Administrador")
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los permisos del rol Administrador están protegidos y no pueden modificarse."
                });
            }

            dto.Modulos ??= [];

            var modulosRepetidos = dto.Modulos
                .GroupBy(m =>
                    m.IdModulo)
                .Where(g =>
                    g.Count() > 1)
                .Select(g =>
                    g.Key)
                .ToList();

            if (modulosRepetidos.Count > 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede enviar el mismo módulo más de una vez."
                });
            }

            var idsModulos = dto.Modulos
                .Select(m =>
                    m.IdModulo)
                .Distinct()
                .ToList();

            if (idsModulos.Count > 0)
            {
                var cantidadModulosValidos =
                    await _context.Modulos
                        .CountAsync(m =>
                            idsModulos.Contains(
                                m.IdModulo) &&
                            m.Estado);

                if (cantidadModulosValidos !=
                    idsModulos.Count)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Uno o más módulos no existen o están inactivos."
                    });
                }
            }

            var idsOperaciones = dto.Modulos
                .SelectMany(m =>
                    m.IdsOperaciones ?? [])
                .Distinct()
                .ToList();

            if (idsOperaciones.Count > 0)
            {
                var cantidadOperacionesValidas =
                    await _context.Operaciones
                        .CountAsync(o =>
                            idsOperaciones.Contains(
                                o.IdOperacion));

                if (cantidadOperacionesValidas !=
                    idsOperaciones.Count)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Una o más operaciones seleccionadas no existen."
                    });
                }
            }

            var consultar =
                await _context.Operaciones
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o =>
                        o.Nombre ==
                        "Consultar");

            if (consultar == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No existe la operación base Consultar."
                });
            }

            foreach (var modulo in dto.Modulos)
            {
                modulo.IdsOperaciones ??= [];

                modulo.IdsOperaciones =
                    modulo.IdsOperaciones
                        .Distinct()
                        .ToList();

                if (modulo.IdsOperaciones.Count == 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Cada módulo agregado debe tener al menos una operación."
                    });
                }

                if (!modulo.IdsOperaciones.Contains(
                        consultar.IdOperacion))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Todo módulo con acceso debe incluir la operación Consultar para que pueda aparecer en el menú."
                    });
                }
            }

            var solicitados = dto.Modulos
                .SelectMany(
                    modulo =>
                        modulo.IdsOperaciones.Select(
                            idOperacion =>
                                (
                                    IdModulo:
                                        modulo.IdModulo,

                                    IdOperacion:
                                        idOperacion
                                )))
                .ToHashSet();

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var actuales =
                    await _context.RolPermisos
                        .Where(rp =>
                            rp.IdRol == idRol)
                        .ToListAsync();

                var actualesSet = actuales
                    .Select(rp =>
                        (
                            rp.IdModulo,
                            rp.IdOperacion
                        ))
                    .ToHashSet();

                var eliminar = actuales
                    .Where(rp =>
                        !solicitados.Contains(
                            (
                                rp.IdModulo,
                                rp.IdOperacion
                            )))
                    .ToList();

                if (eliminar.Count > 0)
                {
                    _context.RolPermisos
                        .RemoveRange(eliminar);
                }

                var agregar = solicitados
                    .Where(p =>
                        !actualesSet.Contains(p))
                    .Select(p =>
                        new RolPermiso
                        {
                            IdRol =
                                idRol,

                            IdModulo =
                                p.IdModulo,

                            IdOperacion =
                                p.IdOperacion
                        })
                    .ToList();

                if (agregar.Count > 0)
                {
                    await _context.RolPermisos
                        .AddRangeAsync(agregar);
                }

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                return Ok(new
                {
                    mensaje =
                        "Permisos aplicados correctamente.",

                    IdRol =
                        idRol,

                    Rol =
                        rol.Nombre,

                    CantidadPermisos =
                        solicitados.Count,

                    CantidadModulos =
                        dto.Modulos.Count,

                    Agregados =
                        agregar.Count,

                    Eliminados =
                        eliminar.Count
                });
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // CONSULTAR TODOS LOS REGISTROS INDIVIDUALES
        // Se conserva para compatibilidad.
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetPermisos()
        {
            var permisos =
                await _context.RolPermisos
                    .Include(rp =>
                        rp.Rol)
                    .Include(rp =>
                        rp.Modulo)
                    .Include(rp =>
                        rp.Operacion)
                    .OrderBy(rp =>
                        rp.Rol!.Nombre)
                    .ThenBy(rp =>
                        rp.Modulo!.Nombre)
                    .ThenBy(rp =>
                        rp.Operacion!.Nombre)
                    .Select(rp => new
                    {
                        rp.IdRolPermiso,
                        rp.IdRol,

                        Rol =
                            rp.Rol != null
                                ? rp.Rol.Nombre
                                : null,

                        rp.IdModulo,

                        Modulo =
                            rp.Modulo != null
                                ? rp.Modulo.Nombre
                                : null,

                        rp.IdOperacion,

                        Operacion =
                            rp.Operacion != null
                                ? rp.Operacion.Nombre
                                : null
                    })
                    .ToListAsync();

            return Ok(permisos);
        }

        // =========================================================
        // CONSULTAR REGISTRO INDIVIDUAL
        // =========================================================

        [HttpGet("{id}")]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetPermiso(
            int id)
        {
            var permiso =
                await _context.RolPermisos
                    .Include(rp =>
                        rp.Rol)
                    .Include(rp =>
                        rp.Modulo)
                    .Include(rp =>
                        rp.Operacion)
                    .Where(rp =>
                        rp.IdRolPermiso == id)
                    .Select(rp => new
                    {
                        rp.IdRolPermiso,
                        rp.IdRol,

                        Rol =
                            rp.Rol != null
                                ? rp.Rol.Nombre
                                : null,

                        rp.IdModulo,

                        Modulo =
                            rp.Modulo != null
                                ? rp.Modulo.Nombre
                                : null,

                        rp.IdOperacion,

                        Operacion =
                            rp.Operacion != null
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

        // =========================================================
        // CONSULTAR PERMISOS DE UN ROL
        // =========================================================

        [HttpGet("rol/{idRol}")]
        [Authorize(Policy = "RolPermisosConsultar")]
        public async Task<IActionResult> GetPorRol(
            int idRol)
        {
            var rolExiste =
                await _context.Roles
                    .AnyAsync(r =>
                        r.IdRol == idRol);

            if (!rolExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        "El rol indicado no existe."
                });
            }

            var permisos =
                await _context.RolPermisos
                    .Include(rp =>
                        rp.Modulo)
                    .Include(rp =>
                        rp.Operacion)
                    .Where(rp =>
                        rp.IdRol == idRol)
                    .OrderBy(rp =>
                        rp.Modulo!.Nombre)
                    .ThenBy(rp =>
                        rp.Operacion!.Nombre)
                    .Select(rp => new
                    {
                        rp.IdRolPermiso,
                        rp.IdRol,
                        rp.IdModulo,

                        Modulo =
                            rp.Modulo != null
                                ? rp.Modulo.Nombre
                                : null,

                        rp.IdOperacion,

                        Operacion =
                            rp.Operacion != null
                                ? rp.Operacion.Nombre
                                : null
                    })
                    .ToListAsync();

            return Ok(permisos);
        }

        // =========================================================
        // CREAR PERMISO INDIVIDUAL
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "RolPermisosCrear")]
        public async Task<IActionResult> CrearPermiso(
            RolPermiso permiso)
        {
            var rol =
                await _context.Roles
                    .FirstOrDefaultAsync(r =>
                        r.IdRol ==
                            permiso.IdRol);

            if (rol == null ||
                !rol.Estado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El rol no existe o está inactivo."
                });
            }

            if (rol.Nombre == "Administrador")
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los permisos del rol Administrador están protegidos."
                });
            }

            var moduloExiste =
                await _context.Modulos
                    .AnyAsync(m =>
                        m.IdModulo ==
                            permiso.IdModulo &&
                        m.Estado);

            if (!moduloExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El módulo no existe o está inactivo."
                });
            }

            var operacionExiste =
                await _context.Operaciones
                    .AnyAsync(o =>
                        o.IdOperacion ==
                            permiso.IdOperacion);

            if (!operacionExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La operación indicada no existe."
                });
            }

            var duplicado =
                await _context.RolPermisos
                    .AnyAsync(rp =>
                        rp.IdRol ==
                            permiso.IdRol &&
                        rp.IdModulo ==
                            permiso.IdModulo &&
                        rp.IdOperacion ==
                            permiso.IdOperacion);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El permiso ya está asignado al rol."
                });
            }

            _context.RolPermisos.Add(
                permiso);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPermiso),
                new
                {
                    id =
                        permiso.IdRolPermiso
                },
                new
                {
                    permiso.IdRolPermiso,
                    permiso.IdRol,
                    permiso.IdModulo,
                    permiso.IdOperacion
                });
        }

        // =========================================================
        // MODIFICAR PERMISO INDIVIDUAL
        // =========================================================

        [HttpPut("{id}")]
        [Authorize(Policy = "RolPermisosModificar")]
        public async Task<IActionResult> ModificarPermiso(
            int id,
            RolPermiso permiso)
        {
            if (id !=
                permiso.IdRolPermiso)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del permiso."
                });
            }

            var existente =
                await _context.RolPermisos
                    .Include(rp =>
                        rp.Rol)
                    .FirstOrDefaultAsync(rp =>
                        rp.IdRolPermiso == id);

            if (existente == null)
            {
                return NotFound();
            }

            if (existente.Rol?.Nombre ==
                "Administrador")
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pueden modificar los permisos del rol Administrador."
                });
            }

            var rol =
                await _context.Roles
                    .FirstOrDefaultAsync(r =>
                        r.IdRol ==
                            permiso.IdRol &&
                        r.Estado);

            if (rol == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El rol no existe o está inactivo."
                });
            }

            if (rol.Nombre ==
                "Administrador")
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los permisos del rol Administrador están protegidos."
                });
            }

            var moduloExiste =
                await _context.Modulos
                    .AnyAsync(m =>
                        m.IdModulo ==
                            permiso.IdModulo &&
                        m.Estado);

            if (!moduloExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El módulo no existe o está inactivo."
                });
            }

            var operacionExiste =
                await _context.Operaciones
                    .AnyAsync(o =>
                        o.IdOperacion ==
                            permiso.IdOperacion);

            if (!operacionExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La operación indicada no existe."
                });
            }

            var duplicado =
                await _context.RolPermisos
                    .AnyAsync(rp =>
                        rp.IdRol ==
                            permiso.IdRol &&
                        rp.IdModulo ==
                            permiso.IdModulo &&
                        rp.IdOperacion ==
                            permiso.IdOperacion &&
                        rp.IdRolPermiso != id);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe esa combinación de rol, módulo y operación."
                });
            }

            existente.IdRol =
                permiso.IdRol;

            existente.IdModulo =
                permiso.IdModulo;

            existente.IdOperacion =
                permiso.IdOperacion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdRolPermiso,
                existente.IdRol,
                existente.IdModulo,
                existente.IdOperacion
            });
        }

        // =========================================================
        // ELIMINAR PERMISO INDIVIDUAL
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize(Policy = "RolPermisosEliminar")]
        public async Task<IActionResult> EliminarPermiso(
            int id)
        {
            var permiso =
                await _context.RolPermisos
                    .Include(rp =>
                        rp.Rol)
                    .FirstOrDefaultAsync(rp =>
                        rp.IdRolPermiso == id);

            if (permiso == null)
            {
                return NotFound();
            }

            if (permiso.Rol?.Nombre ==
                "Administrador")
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pueden eliminar permisos del rol Administrador."
                });
            }

            _context.RolPermisos.Remove(
                permiso);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static int OrdenOperacion(
            string nombre)
        {
            return nombre switch
            {
                "Consultar" => 1,
                "Crear" => 2,
                "Modificar" => 3,
                "Eliminar" => 4,
                _ => 100
            };
        }
    }
}