namespace Clinica.API.Models;

public class DetalleFactura
{
    public int IdDetalleFactura { get; set; }

    public int IdFactura { get; set; }

    public int IdCatalogo { get; set; }

    public int? IdAsignacionHabitacion { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Total { get; set; }

    public Factura? Factura { get; set; }

    public Catalogo? Catalogo { get; set; }

    public AsignacionHabitacion? AsignacionHabitacion { get; set; }
}