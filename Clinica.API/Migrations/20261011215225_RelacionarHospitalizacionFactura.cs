using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class RelacionarHospitalizacionFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAsignacionHabitacion",
                table: "DetalleFactura",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_IdAsignacionHabitacion",
                table: "DetalleFactura",
                column: "IdAsignacionHabitacion");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleFactura_AsignacionesHabitacion_IdAsignacionHabitacion",
                table: "DetalleFactura",
                column: "IdAsignacionHabitacion",
                principalTable: "AsignacionesHabitacion",
                principalColumn: "IdAsignacion",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleFactura_AsignacionesHabitacion_IdAsignacionHabitacion",
                table: "DetalleFactura");

            migrationBuilder.DropIndex(
                name: "IX_DetalleFactura_IdAsignacionHabitacion",
                table: "DetalleFactura");

            migrationBuilder.DropColumn(
                name: "IdAsignacionHabitacion",
                table: "DetalleFactura");
        }
    }
}
