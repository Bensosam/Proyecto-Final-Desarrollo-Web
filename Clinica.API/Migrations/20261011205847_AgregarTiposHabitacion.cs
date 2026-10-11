using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTiposHabitacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadCamas",
                table: "Habitaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoHabitacion",
                table: "Habitaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposHabitacion",
                columns: table => new
                {
                    IdTipoHabitacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PrecioPorDia = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposHabitacion", x => x.IdTipoHabitacion);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_IdTipoHabitacion",
                table: "Habitaciones",
                column: "IdTipoHabitacion");

            migrationBuilder.AddForeignKey(
                name: "FK_Habitaciones_TiposHabitacion_IdTipoHabitacion",
                table: "Habitaciones",
                column: "IdTipoHabitacion",
                principalTable: "TiposHabitacion",
                principalColumn: "IdTipoHabitacion",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habitaciones_TiposHabitacion_IdTipoHabitacion",
                table: "Habitaciones");

            migrationBuilder.DropTable(
                name: "TiposHabitacion");

            migrationBuilder.DropIndex(
                name: "IX_Habitaciones_IdTipoHabitacion",
                table: "Habitaciones");

            migrationBuilder.DropColumn(
                name: "CantidadCamas",
                table: "Habitaciones");

            migrationBuilder.DropColumn(
                name: "IdTipoHabitacion",
                table: "Habitaciones");
        }
    }
}
