namespace Clinica.API.Models;

public class Catalogo
{
    public int IdCatalogo { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public bool Estado { get; set; } = true;

    public int? IdMedicamento { get; set; }

    public int? IdTipoHabitacion { get; set; }

    public Medicamento? Medicamento { get; set; }

    public TipoHabitacion? TipoHabitacion { get; set; }

    public ICollection<DetalleFactura> DetallesFactura { get; set; }
        = new List<DetalleFactura>();
}