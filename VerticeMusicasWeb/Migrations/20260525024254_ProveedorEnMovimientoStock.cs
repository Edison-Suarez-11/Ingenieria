using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticeMuiscaWeb.Migrations
{
    /// <inheritdoc />
    public partial class ProveedorEnMovimientoStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "idProveedor",
                table: "Stock",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precioUnitarioCompra",
                table: "Stock",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stock_idProveedor",
                table: "Stock",
                column: "idProveedor");

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_Proveedor_idProveedor",
                table: "Stock",
                column: "idProveedor",
                principalTable: "Proveedor",
                principalColumn: "idProveedor",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_Proveedor_idProveedor",
                table: "Stock");

            migrationBuilder.DropIndex(
                name: "IX_Stock_idProveedor",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "idProveedor",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "precioUnitarioCompra",
                table: "Stock");
        }
    }
}
