using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticeMuiscaWeb.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracionAlertaStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL idempotente: snapshots anteriores no incluían todo el esquema; evitamos repetir columnas/tablas ya creadas por migraciones previas o por EnsureSprint3Schema.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ConfiguracionAlertaStock (
                    idConfig INTEGER NOT NULL PRIMARY KEY CHECK (idConfig = 1),
                    canal INTEGER NOT NULL DEFAULT 0,
                    destino TEXT NOT NULL DEFAULT '',
                    activo INTEGER NOT NULL DEFAULT 0
                );
                INSERT OR IGNORE INTO ConfiguracionAlertaStock (idConfig, canal, destino, activo) VALUES (1, 0, '', 0);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ConfiguracionAlertaStock;");
        }
    }
}
