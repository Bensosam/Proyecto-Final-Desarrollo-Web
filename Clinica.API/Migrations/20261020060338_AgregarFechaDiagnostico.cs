using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaDiagnostico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<DateTime>(
        name: "FechaDiagnostico",
        table: "Diagnosticos",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(
            1, 1, 1,
            0, 0, 0, 0,
            DateTimeKind.Unspecified));

    // Los diagnósticos ya existentes toman la fecha
    // de la consulta a la que pertenecen.
    migrationBuilder.Sql(
        """
        UPDATE D
        SET D.FechaDiagnostico = C.FechaConsulta
        FROM Diagnosticos AS D
        INNER JOIN Consultas AS C
            ON C.IdConsulta = D.IdConsulta;
        """
    );
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaDiagnostico",
                table: "Diagnosticos");
        }
    }
}
