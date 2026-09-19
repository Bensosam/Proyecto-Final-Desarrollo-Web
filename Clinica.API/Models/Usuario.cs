namespace Clinica.API.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    public int IdEmpleado { get; set; }

    public int IdRol { get; set; }

    public string UsuarioLogin { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public Empleado? Empleado { get; set; }

    public Rol? Rol { get; set; }
}