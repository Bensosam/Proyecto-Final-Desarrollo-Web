using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLecturaResultadosYPdfExamenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ArchivoPdf",
                table: "Examenes",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreArchivoPdf",
                table: "Examenes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LecturaResultadosExamenes",
                table: "Consultas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivoPdf",
                table: "Examenes");

            migrationBuilder.DropColumn(
                name: "NombreArchivoPdf",
                table: "Examenes");

            migrationBuilder.DropColumn(
                name: "LecturaResultadosExamenes",
                table: "Consultas");
        }
    }
}
