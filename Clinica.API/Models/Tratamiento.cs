namespace Clinica.API.Models;

public class Tratamiento
{
    public int IdTratamiento { get; set; }

    public int IdConsulta { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Indicaciones { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public Consulta? Consulta { get; set; }
}