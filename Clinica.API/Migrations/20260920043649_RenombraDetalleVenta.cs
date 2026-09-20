using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class RenombraDetalleVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_LotesMedicamento_IdLote",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_VentasFarmacia_IdVenta",
                table: "DetallesVenta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesVenta",
                table: "DetallesVenta");

            migrationBuilder.RenameTable(
                name: "DetallesVenta",
                newName: "DetalleVenta");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_IdVenta",
                table: "DetalleVenta",
                newName: "IX_DetalleVenta_IdVenta");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_IdLote",
                table: "DetalleVenta",
                newName: "IX_DetalleVenta_IdLote");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleVenta",
                table: "DetalleVenta",
                column: "IdDetalle");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVenta_LotesMedicamento_IdLote",
                table: "DetalleVenta",
                column: "IdLote",
                principalTable: "LotesMedicamento",
                principalColumn: "IdLote",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVenta_VentasFarmacia_IdVenta",
                table: "DetalleVenta",
                column: "IdVenta",
                principalTable: "VentasFarmacia",
                principalColumn: "IdVenta",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVenta_LotesMedicamento_IdLote",
                table: "DetalleVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVenta_VentasFarmacia_IdVenta",
                table: "DetalleVenta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleVenta",
                table: "DetalleVenta");

            migrationBuilder.RenameTable(
                name: "DetalleVenta",
                newName: "DetallesVenta");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVenta_IdVenta",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_IdVenta");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVenta_IdLote",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_IdLote");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesVenta",
                table: "DetallesVenta",
                column: "IdDetalle");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_LotesMedicamento_IdLote",
                table: "DetallesVenta",
                column: "IdLote",
                principalTable: "LotesMedicamento",
                principalColumn: "IdLote",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_VentasFarmacia_IdVenta",
                table: "DetallesVenta",
                column: "IdVenta",
                principalTable: "VentasFarmacia",
                principalColumn: "IdVenta",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
