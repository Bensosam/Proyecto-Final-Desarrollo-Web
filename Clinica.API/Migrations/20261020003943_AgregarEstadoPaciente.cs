using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Pacientes",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pacientes");
        }
    }
}
