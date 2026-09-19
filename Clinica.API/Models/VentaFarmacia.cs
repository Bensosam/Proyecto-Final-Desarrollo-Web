namespace Clinica.API.Models;

public class VentaFarmacia
{
    public int IdVenta { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaVenta { get; set; }

    public decimal Total { get; set; }

    public Usuario? Usuario { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; }
        = new List<DetalleVenta>();
}