namespace Clinica.API.Models;

public class Marca
{
    public int IdMarca { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<Medicamento> Medicamentos { get; set; }
        = new List<Medicamento>();
}