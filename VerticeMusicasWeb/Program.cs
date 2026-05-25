using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Services;

var builder = WebApplication.CreateBuilder(args);
string dbPath = ResolveDatabasePath(builder.Environment.ContentRootPath);

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<InventarioStockService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<InformesService>();
builder.Services.AddScoped<ProveedorService>();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    EnsureSprint3Schema(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

static void EnsureSprint3Schema(AppDbContext db)
{
    using var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
    {
        connection.Open();
    }

    EnsureColumnExists(connection, "Producto", "manejaStock", "INTEGER NOT NULL DEFAULT 1");
    EnsureColumnExists(connection, "Stock", "stockMinimo", "INTEGER NOT NULL DEFAULT 0");

    ExecuteNonQuery(connection, @"
        CREATE TABLE IF NOT EXISTS Venta (
            idVenta INTEGER PRIMARY KEY AUTOINCREMENT,
            fecha TEXT NOT NULL,
            total REAL NOT NULL,
            metodoPago TEXT NOT NULL
        );");

    ExecuteNonQuery(connection, @"
        CREATE TABLE IF NOT EXISTS DetalleVenta (
            idDetalle INTEGER PRIMARY KEY AUTOINCREMENT,
            idVenta INTEGER NOT NULL,
            idProducto INTEGER NOT NULL,
            cantidad INTEGER NOT NULL,
            precioUnitario REAL NOT NULL,
            FOREIGN KEY(idVenta) REFERENCES Venta(idVenta) ON DELETE CASCADE,
            FOREIGN KEY(idProducto) REFERENCES Producto(idProducto) ON DELETE RESTRICT
        );");

    ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS IX_DetalleVenta_idVenta ON DetalleVenta(idVenta);");
    ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS IX_DetalleVenta_idProducto ON DetalleVenta(idProducto);");
}

static void EnsureColumnExists(System.Data.Common.DbConnection connection, string tableName, string columnName, string columnDefinition)
{
    bool exists = false;
    using (var command = connection.CreateCommand())
    {
        command.CommandText = $"PRAGMA table_info({tableName});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string? name = reader["name"]?.ToString();
            if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }
    }

    if (!exists)
    {
        ExecuteNonQuery(connection, $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};");
    }
}

static void ExecuteNonQuery(System.Data.Common.DbConnection connection, string sql)
{
    using var command = connection.CreateCommand();
    command.CommandText = sql;
    command.ExecuteNonQuery();
}

static string ResolveDatabasePath(string contentRootPath)
{
    string target = Path.Combine(contentRootPath, "vertice_musicas.db");
    string currentDir = Path.Combine(Directory.GetCurrentDirectory(), "vertice_musicas.db");
    string baseDir = Path.Combine(AppContext.BaseDirectory, "vertice_musicas.db");

    // Mantiene una sola BD canonical en la carpeta del proyecto web.
    if (!File.Exists(target))
    {
        if (File.Exists(currentDir))
        {
            File.Copy(currentDir, target, overwrite: true);
        }
        else if (File.Exists(baseDir))
        {
            File.Copy(baseDir, target, overwrite: true);
        }
    }
    else
    {
        if (File.Exists(currentDir) && File.GetLastWriteTimeUtc(currentDir) > File.GetLastWriteTimeUtc(target))
        {
            File.Copy(currentDir, target, overwrite: true);
        }
        else if (File.Exists(baseDir) && File.GetLastWriteTimeUtc(baseDir) > File.GetLastWriteTimeUtc(target))
        {
            File.Copy(baseDir, target, overwrite: true);
        }
    }

    return target;
}
