using Microsoft.AspNetCore.Authorization;

namespace Clinica.API.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Modulo { get; }
        public string Operacion { get; }

        public PermissionRequirement(string modulo, string operacion)
        {
            Modulo = modulo;
            Operacion = operacion;
        }
    }
}
