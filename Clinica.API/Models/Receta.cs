namespace Clinica.API.Models;

public class Receta
{
    public int IdReceta { get; set; }

    public int IdConsulta { get; set; }

    public DateTime FechaEmision { get; set; }

    public string Observaciones { get; set; } = string.Empty;

    public Consulta? Consulta { get; set; }

    public ICollection<DetalleReceta> Detalles { get; set; }
        = new List<DetalleReceta>();
}