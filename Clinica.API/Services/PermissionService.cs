using Clinica.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Services
{
    public class PermissionService
    {
        private readonly ClinicaDbContext _context;

        public PermissionService(ClinicaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(
            int idUsuario,
            string modulo,
            string operacion)
        {
            return await _context.RolPermisos
                .AnyAsync(rp =>
                    rp.IdRol == _context.Usuarios
                        .Where(u => u.IdUsuario == idUsuario && u.Estado)
                        .Select(u => u.IdRol)
                        .FirstOrDefault() &&
                    rp.Modulo != null &&
                    rp.Modulo.Nombre == modulo &&
                    rp.Modulo.Estado &&
                    rp.Operacion != null &&
                    rp.Operacion.Nombre == operacion);
        }
    }
}
