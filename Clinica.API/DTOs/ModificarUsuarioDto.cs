namespace Clinica.API.DTOs;

public class ModificarUsuarioDto
{
    public int IdUsuario { get; set; }

    public int IdEmpleado { get; set; }

    public int IdRol { get; set; }

    public string UsuarioLogin { get; set; } = string.Empty;

    public string? Password { get; set; }

    public bool Estado { get; set; }
}