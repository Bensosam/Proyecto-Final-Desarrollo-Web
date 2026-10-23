namespace Clinica.API.Models;

public class ContactoEmergencia
{
    public int IdContacto { get; set; }

    public int IdPaciente { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Parentesco { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    // Se conserva por compatibilidad con datos anteriores.
    public string TelefonoAlternativo { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public Paciente? Paciente { get; set; }
}
