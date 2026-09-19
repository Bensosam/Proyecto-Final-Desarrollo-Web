namespace Clinica.API.Models;

public class Diagnostico
{
    public int IdDiagnostico { get; set; }

    public int IdConsulta { get; set; }

    public string DiagnosticoTexto { get; set; } = string.Empty;

    public string Observaciones { get; set; } = string.Empty;

    public Consulta? Consulta { get; set; }
}