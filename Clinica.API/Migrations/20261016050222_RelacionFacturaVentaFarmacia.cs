using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class RelacionFacturaVentaFarmacia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdPaciente",
                table: "Facturas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IdVenta",
                table: "Facturas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdVenta",
                table: "Facturas",
                column: "IdVenta",
                unique: true,
                filter: "[IdVenta] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_VentasFarmacia_IdVenta",
                table: "Facturas",
                column: "IdVenta",
                principalTable: "VentasFarmacia",
                principalColumn: "IdVenta",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_VentasFarmacia_IdVenta",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_IdVenta",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "IdVenta",
                table: "Facturas");

            migrationBuilder.AlterColumn<int>(
                name: "IdPaciente",
                table: "Facturas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
