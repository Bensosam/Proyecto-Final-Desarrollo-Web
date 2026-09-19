namespace Clinica.API.Models;

public class DetalleVenta
{
    public int IdDetalle { get; set; }

    public int IdVenta { get; set; }

    public int IdLote { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Total { get; set; }

    public VentaFarmacia? Venta { get; set; }

    public LoteMedicamento? Lote { get; set; }
}