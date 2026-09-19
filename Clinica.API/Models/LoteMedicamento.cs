namespace Clinica.API.Models;

public class LoteMedicamento
{
    public int IdLote { get; set; }

    public int IdMedicamento { get; set; }

    public string NumeroLote { get; set; } = string.Empty;

    public DateTime FechaIngreso { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public int CantidadDisponible { get; set; }

    public Medicamento? Medicamento { get; set; }

    public ICollection<MovimientoInventario> Movimientos { get; set; }
        = new List<MovimientoInventario>();

    public ICollection<DetalleVenta> DetallesVenta { get; set; }
        = new List<DetalleVenta>();
}