namespace Clinica.API.Models;

public class Factura
{
    public int IdFactura { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public int IdPaciente { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaFactura { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "Emitida";

    public Paciente? Paciente { get; set; }

    public Usuario? Usuario { get; set; }

    public ICollection<DetalleFactura> Detalles { get; set; }
        = new List<DetalleFactura>();
}