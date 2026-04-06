using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticeMuiscaWeb.Migrations
{
    /// <inheritdoc />
    public partial class SchemaCompletoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    idCategoria = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombreCategoria = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.idCategoria);
                });

            migrationBuilder.CreateTable(
                name: "Inventario",
                columns: table => new
                {
                    idInventario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fecha = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventario", x => x.idInventario);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    idProducto = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    codigoBarras = table.Column<string>(type: "TEXT", nullable: false),
                    precio = table.Column<decimal>(type: "TEXT", nullable: false),
                    marca = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    idCategoria = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.idProducto);
                    table.ForeignKey(
                        name: "FK_Producto_Categoria_idCategoria",
                        column: x => x.idCategoria,
                        principalTable: "Categoria",
                        principalColumn: "idCategoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    idStock = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    idInventario = table.Column<int>(type: "INTEGER", nullable: false),
                    idProducto = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.idStock);
                    table.ForeignKey(
                        name: "FK_Stock_Inventario_idInventario",
                        column: x => x.idInventario,
                        principalTable: "Inventario",
                        principalColumn: "idInventario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stock_Producto_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Producto",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Producto_codigoBarras",
                table: "Producto",
                column: "codigoBarras",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producto_idCategoria",
                table: "Producto",
                column: "idCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_idInventario",
                table: "Stock",
                column: "idInventario");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_idProducto",
                table: "Stock",
                column: "idProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "Inventario");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Categoria");
        }
    }
}
