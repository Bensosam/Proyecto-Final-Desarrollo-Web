namespace Clinica.API.DTOs;

public class CrearUsuarioDto
{
    public int IdEmpleado { get; set; }

    public int IdRol { get; set; }

    public string UsuarioLogin { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;
}