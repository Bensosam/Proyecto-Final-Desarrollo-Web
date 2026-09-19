namespace Clinica.API.Models;

public class Examen
{
    public int IdExamen { get; set; }

    public int IdConsulta { get; set; }

    public string NombreExamen { get; set; } = string.Empty;

    public DateTime FechaExamen { get; set; }

    public string Resultado { get; set; } = string.Empty;

    public string Observaciones { get; set; } = string.Empty;

    public Consulta? Consulta { get; set; }
}