namespace Clinica.API.Models;

public class MovimientoInventario
{
    public int IdMovimiento { get; set; }

    public int IdLote { get; set; }

    public int IdUsuario { get; set; }

    public string TipoMovimiento { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public LoteMedicamento? Lote { get; set; }

    public Usuario? Usuario { get; set; }
}