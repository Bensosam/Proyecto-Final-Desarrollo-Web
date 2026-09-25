using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Clinica.API.Services;

namespace Clinica.API.Authorization
{
    public class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly PermissionService _permissionService;

        public PermissionAuthorizationHandler(
            PermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var idUsuarioClaim = context.User.FindFirst(
                ClaimTypes.NameIdentifier);

            if (idUsuarioClaim == null)
                return;

            if (!int.TryParse(idUsuarioClaim.Value, out int idUsuario))
                return;

            bool tienePermiso =
                await _permissionService.HasPermissionAsync(
                    idUsuario,
                    requirement.Modulo,
                    requirement.Operacion);

            if (tienePermiso)
            {
                context.Succeed(requirement);
            }
        }
    }
}
