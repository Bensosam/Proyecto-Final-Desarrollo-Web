using System.ComponentModel.DataAnnotations.Schema;

namespace Clinica.API.Models;

public class Examen
{
    public int IdExamen { get; set; }

    public int? IdDiagnostico { get; set; }

    // Se obtiene automáticamente desde el diagnóstico.
    public int IdConsulta { get; set; }

    public string NombreExamen { get; set; } = string.Empty;

    public DateTime FechaExamen { get; set; }

    public string Resultado { get; set; } = string.Empty;

    public string Observaciones { get; set; } = string.Empty;

    // PDF almacenado en base de datos.
    public byte[]? ArchivoPdf { get; set; }

    public string? NombreArchivoPdf { get; set; }

    // Solo se utiliza para recibir el archivo desde Blazor.
    [NotMapped]
    public string? ArchivoPdfBase64 { get; set; }

    public Diagnostico? Diagnostico { get; set; }

    public Consulta? Consulta { get; set; }
}