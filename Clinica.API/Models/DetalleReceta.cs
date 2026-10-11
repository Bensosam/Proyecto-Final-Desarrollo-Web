namespace Clinica.API.Models;

public class DetalleReceta
{
    public int IdDetalleReceta { get; set; }

    public int IdReceta { get; set; }

    public int IdMedicamento { get; set; }

    public int Cantidad { get; set; }

    public string Dosis { get; set; } = string.Empty;

    public string Frecuencia { get; set; } = string.Empty;

    public int DuracionDias { get; set; }

    public string Indicaciones { get; set; } = string.Empty;

    public Receta? Receta { get; set; }

    public Medicamento? Medicamento { get; set; }
}