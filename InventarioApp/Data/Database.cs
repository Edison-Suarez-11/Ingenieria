using System.Data.SQLite;
using System.Globalization;
using System.Windows.Forms;
using InventarioApp.Models;

namespace InventarioApp.Data;

public static class Database
{
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventario.db");
    private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

    private static SQLiteConnection CrearConexion()
    {
        var connection = new SQLiteConnection(ConnectionString);
        connection.Open();
        // Asegura que las llaves foraneas se apliquen correctamente.
        using SQLiteCommand pragma = new("PRAGMA foreign_keys = ON;", connection);
        pragma.ExecuteNonQuery();
        return connection;
    }

    public static void InitializeDatabase()
    {
        try
        {
            using SQLiteConnection connection = CrearConexion();

            using SQLiteCommand command = connection.CreateCommand();

            // Categoria (Sprint 1)
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Categoria (
                    idCategoria INTEGER PRIMARY KEY AUTOINCREMENT,
                    nombreCategoria TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            // Producto (Sprint 1)
            // Nota: si ya existía una versión anterior de Producto, haremos migración simple con ALTER TABLE.
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Producto (
                    idProducto INTEGER PRIMARY KEY AUTOINCREMENT,
                    nombre TEXT NOT NULL,
                    codigoBarras TEXT NOT NULL UNIQUE,
                    precio REAL NOT NULL,
                    marca TEXT,
                    idCategoria INTEGER NOT NULL,
                    FOREIGN KEY(idCategoria) REFERENCES Categoria(idCategoria)
                );";
            command.ExecuteNonQuery();

            // Migración: agregar columnas faltantes si existían tablas creadas previamente sin precio/marca.
            // (No podemos retroajustar constraints NOT NULL con ALTER TABLE de forma perfecta en SQLite.)
            EnsureProductoColumn(connection, "precio", "REAL NOT NULL DEFAULT 0");
            EnsureProductoColumn(connection, "marca", "TEXT");

            // Asegura uniqueness para codigoBarras (si ya existía la tabla sin UNIQUE).
            try
            {
                command.CommandText = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_producto_codigobarras_unique
                    ON Producto(codigoBarras);";
                command.ExecuteNonQuery();
            }
            catch
            {
                // Si existen duplicados previos, el index fallará. La aplicación los validará en Services.
            }

            // Inventario (Sprint 2)
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Inventario (
                    idInventario INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            // Stock como movimiento/delta: cada registro representa una entrada (inicial o adicional) para un producto.
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Stock (
                    idStock INTEGER PRIMARY KEY AUTOINCREMENT,
                    cantidad INTEGER NOT NULL,
                    idInventario INTEGER NOT NULL,
                    idProducto INTEGER NOT NULL,
                    FOREIGN KEY(idInventario) REFERENCES Inventario(idInventario),
                    FOREIGN KEY(idProducto) REFERENCES Producto(idProducto)
                );";
            command.ExecuteNonQuery();

            // Índices (rendimiento)
            command.CommandText = "CREATE INDEX IF NOT EXISTS idx_stock_idProducto ON Stock(idProducto);";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE INDEX IF NOT EXISTS idx_stock_idInventario ON Stock(idInventario);";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE INDEX IF NOT EXISTS idx_inventario_fecha ON Inventario(fecha);";
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al inicializar la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void EnsureProductoColumn(SQLiteConnection connection, string columnName, string columnDefinition)
    {
        using SQLiteCommand infoCmd = new("PRAGMA table_info(Producto);", connection);
        using SQLiteDataReader reader = infoCmd.ExecuteReader();

        bool exists = false;
        while (reader.Read())
        {
            string? name = reader["name"]?.ToString();
            if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            using SQLiteCommand alter = new($"ALTER TABLE Producto ADD COLUMN {columnName} {columnDefinition};", connection);
            alter.ExecuteNonQuery();
        }
    }

    public static List<Categoria> GetCategorias()
    {
        List<Categoria> categorias = [];

        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = new("SELECT idCategoria, nombreCategoria FROM Categoria ORDER BY idCategoria DESC;", connection);
        using SQLiteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            categorias.Add(new Categoria
            {
                IdCategoria = Convert.ToInt32(reader["idCategoria"]),
                NombreCategoria = reader["nombreCategoria"].ToString() ?? string.Empty
            });
        }

        return categorias;
    }

    public static List<Categoria> BuscarCategorias(string? term)
    {
        List<Categoria> categorias = [];

        using SQLiteConnection connection = CrearConexion();

        string sql = "SELECT idCategoria, nombreCategoria FROM Categoria";
        if (!string.IsNullOrWhiteSpace(term))
        {
            sql += " WHERE LOWER(nombreCategoria) LIKE @like";
        }
        sql += " ORDER BY idCategoria DESC;";

        using SQLiteCommand command = new(sql, connection);
        if (!string.IsNullOrWhiteSpace(term))
        {
            command.Parameters.AddWithValue("@like", $"%{term.Trim().ToLowerInvariant()}%");
        }

        using SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            categorias.Add(new Categoria
            {
                IdCategoria = Convert.ToInt32(reader["idCategoria"]),
                NombreCategoria = reader["nombreCategoria"].ToString() ?? string.Empty
            });
        }

        return categorias;
    }

    public static bool ExisteCategoriaPorNombre(string nombre, int? idExcluir = null)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = connection.CreateCommand();

        command.CommandText = @"
            SELECT COUNT(1)
            FROM Categoria
            WHERE LOWER(nombreCategoria) = @nombre";
        command.Parameters.AddWithValue("@nombre", nombre.Trim().ToLowerInvariant());

        if (idExcluir.HasValue)
        {
            command.CommandText += " AND idCategoria <> @idExcluir";
            command.Parameters.AddWithValue("@idExcluir", idExcluir.Value);
        }

        long count = (long)(command.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    public static bool ExisteCategoriaPorId(int idCategoria)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = new("SELECT COUNT(1) FROM Categoria WHERE idCategoria = @idCategoria;", connection);
        command.Parameters.AddWithValue("@idCategoria", idCategoria);

        long count = (long)(command.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    public static void InsertCategoria(string nombreCategoria)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = new("INSERT INTO Categoria (nombreCategoria) VALUES (@nombreCategoria);", connection);
        command.Parameters.AddWithValue("@nombreCategoria", nombreCategoria.Trim());
        command.ExecuteNonQuery();
    }

    public static void UpdateCategoria(int idCategoria, string nombreCategoria)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = new("UPDATE Categoria SET nombreCategoria = @nombreCategoria WHERE idCategoria = @idCategoria;", connection);
        command.Parameters.AddWithValue("@nombreCategoria", nombreCategoria.Trim());
        command.Parameters.AddWithValue("@idCategoria", idCategoria);
        command.ExecuteNonQuery();
    }

    public static List<Producto> GetProductos()
    {
        return BuscarProductos(null);
    }

    public static List<Producto> BuscarProductos(string? term)
    {
        List<Producto> productos = [];

        using SQLiteConnection connection = CrearConexion();

        bool filtro = !string.IsNullOrWhiteSpace(term);
        string sql = @"
            SELECT p.idProducto, p.nombre, p.codigoBarras, p.precio, p.marca, p.idCategoria, c.nombreCategoria
            FROM Producto p
            LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria";

        if (filtro)
        {
            sql += @" WHERE p.nombre LIKE @like OR p.codigoBarras = @term OR p.codigoBarras LIKE @like";
        }

        sql += " ORDER BY p.idProducto DESC;";

        using SQLiteCommand command = new(sql, connection);
        if (filtro)
        {
            string normalized = term!.Trim();
            command.Parameters.AddWithValue("@like", $"%{normalized}%");
            command.Parameters.AddWithValue("@term", normalized);
        }

        using SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            productos.Add(new Producto
            {
                IdProducto = Convert.ToInt32(reader["idProducto"]),
                Nombre = reader["nombre"].ToString() ?? string.Empty,
                CodigoBarras = reader["codigoBarras"].ToString() ?? string.Empty,
                Precio = reader["precio"] is DBNull ? 0m : Convert.ToDecimal(reader["precio"], CultureInfo.InvariantCulture),
                Marca = reader["marca"] is DBNull ? string.Empty : reader["marca"].ToString() ?? string.Empty,
                IdCategoria = reader["idCategoria"] is DBNull ? 0 : Convert.ToInt32(reader["idCategoria"]),
                NombreCategoria = reader["nombreCategoria"] is DBNull ? string.Empty : reader["nombreCategoria"].ToString() ?? string.Empty
            });
        }

        return productos;
    }

    public static bool ExisteCodigoBarras(string codigoBarras, int? idExcluir = null)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = connection.CreateCommand();

        command.CommandText = @"
            SELECT COUNT(1)
            FROM Producto
            WHERE codigoBarras = @codigoBarras";
        command.Parameters.AddWithValue("@codigoBarras", codigoBarras.Trim());

        if (idExcluir.HasValue)
        {
            command.CommandText += " AND idProducto <> @idExcluir";
            command.Parameters.AddWithValue("@idExcluir", idExcluir.Value);
        }

        long count = (long)(command.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    public static bool ExisteProductoPorId(int idProducto)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = new("SELECT COUNT(1) FROM Producto WHERE idProducto = @idProducto;", connection);
        command.Parameters.AddWithValue("@idProducto", idProducto);
        long count = (long)(command.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    public static void InsertProducto(string nombre, string codigoBarras, decimal precio, string marca, int idCategoria)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Producto (nombre, codigoBarras, precio, marca, idCategoria)
            VALUES (@nombre, @codigoBarras, @precio, @marca, @idCategoria);";

        command.Parameters.AddWithValue("@nombre", nombre.Trim());
        command.Parameters.AddWithValue("@codigoBarras", codigoBarras.Trim());
        command.Parameters.AddWithValue("@precio", precio);
        command.Parameters.AddWithValue("@marca", string.IsNullOrWhiteSpace(marca) ? DBNull.Value : marca.Trim());
        command.Parameters.AddWithValue("@idCategoria", idCategoria);
        command.ExecuteNonQuery();
    }

    public static void UpdateProducto(int idProducto, string nombre, string codigoBarras, decimal precio, string marca, int idCategoria)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Producto
            SET nombre = @nombre,
                codigoBarras = @codigoBarras,
                precio = @precio,
                marca = @marca,
                idCategoria = @idCategoria
            WHERE idProducto = @idProducto;";

        command.Parameters.AddWithValue("@nombre", nombre.Trim());
        command.Parameters.AddWithValue("@codigoBarras", codigoBarras.Trim());
        command.Parameters.AddWithValue("@precio", precio);
        command.Parameters.AddWithValue("@marca", string.IsNullOrWhiteSpace(marca) ? DBNull.Value : marca.Trim());
        command.Parameters.AddWithValue("@idCategoria", idCategoria);
        command.Parameters.AddWithValue("@idProducto", idProducto);
        command.ExecuteNonQuery();
    }

    private static string FechaToTexto(DateTime fecha)
    {
        // Solo fecha (sin hora) para simplificar visualización y parseo.
        return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static int RegistrarMovimientoInventario(DateTime fecha, int idProducto, int cantidad)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteTransaction tx = connection.BeginTransaction();

        try
        {
            using SQLiteCommand insertInv = connection.CreateCommand();
            insertInv.Transaction = tx;
            insertInv.CommandText = "INSERT INTO Inventario (fecha) VALUES (@fecha);";
            insertInv.Parameters.AddWithValue("@fecha", FechaToTexto(fecha));
            insertInv.ExecuteNonQuery();

            long idInventario = connection.LastInsertRowId;

            using SQLiteCommand insertStock = connection.CreateCommand();
            insertStock.Transaction = tx;
            insertStock.CommandText = @"
                INSERT INTO Stock (cantidad, idInventario, idProducto)
                VALUES (@cantidad, @idInventario, @idProducto);";
            insertStock.Parameters.AddWithValue("@cantidad", cantidad);
            insertStock.Parameters.AddWithValue("@idInventario", idInventario);
            insertStock.Parameters.AddWithValue("@idProducto", idProducto);
            insertStock.ExecuteNonQuery();

            tx.Commit();
            return (int)idInventario;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static int ObtenerStockCantidadActual(int idProducto)
    {
        using SQLiteConnection connection = CrearConexion();
        using SQLiteCommand command = new(@"
            SELECT COALESCE(SUM(cantidad), 0)
            FROM Stock
            WHERE idProducto = @idProducto;", connection);
        command.Parameters.AddWithValue("@idProducto", idProducto);

        long value = (long)(command.ExecuteScalar() ?? 0L);
        return (int)value;
    }

    public static List<InventarioMovimiento> GetInventarioMovimientos(int? idCategoria, string? term)
    {
        List<InventarioMovimiento> movimientos = [];

        using SQLiteConnection connection = CrearConexion();

        bool tieneTerm = !string.IsNullOrWhiteSpace(term);
        string sql = @"
            SELECT i.idInventario, i.fecha, s.cantidad,
                   p.idProducto, p.nombre, p.codigoBarras,
                   c.nombreCategoria
            FROM Stock s
            INNER JOIN Inventario i ON s.idInventario = i.idInventario
            INNER JOIN Producto p ON s.idProducto = p.idProducto
            INNER JOIN Categoria c ON p.idCategoria = c.idCategoria";

        List<string> condiciones = [];
        if (idCategoria.HasValue)
        {
            condiciones.Add("c.idCategoria = @idCategoria");
        }
        if (tieneTerm)
        {
            condiciones.Add("(p.nombre LIKE @like OR p.codigoBarras = @term OR p.codigoBarras LIKE @like)");
        }

        if (condiciones.Count > 0)
        {
            sql += " WHERE " + string.Join(" AND ", condiciones);
        }

        sql += " ORDER BY i.idInventario DESC;";

        using SQLiteCommand command = new(sql, connection);
        if (idCategoria.HasValue)
        {
            command.Parameters.AddWithValue("@idCategoria", idCategoria.Value);
        }

        if (tieneTerm)
        {
            string normalized = term!.Trim();
            command.Parameters.AddWithValue("@like", $"%{normalized}%");
            command.Parameters.AddWithValue("@term", normalized);
        }

        using SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string fechaTexto = reader["fecha"].ToString() ?? "";
            DateTime fecha = DateTime.TryParse(fechaTexto, out var parsed) ? parsed : DateTime.MinValue;

            movimientos.Add(new InventarioMovimiento
            {
                IdInventario = Convert.ToInt32(reader["idInventario"]),
                Fecha = fecha,
                IdProducto = Convert.ToInt32(reader["idProducto"]),
                NombreProducto = reader["nombre"].ToString() ?? string.Empty,
                CodigoBarras = reader["codigoBarras"].ToString() ?? string.Empty,
                NombreCategoria = reader["nombreCategoria"].ToString() ?? string.Empty,
                Cantidad = Convert.ToInt32(reader["cantidad"])
            });
        }

        return movimientos;
    }

    public static List<StockDisponible> GetStockDisponible(int? idCategoria, string? term)
    {
        List<StockDisponible> disponibles = [];

        using SQLiteConnection connection = CrearConexion();

        bool tieneTerm = !string.IsNullOrWhiteSpace(term);

        string sql = @"
            SELECT p.idProducto, p.nombre, p.codigoBarras, c.nombreCategoria,
                   COALESCE(SUM(s.cantidad), 0) AS cantidadDisponible
            FROM Producto p
            INNER JOIN Categoria c ON p.idCategoria = c.idCategoria
            LEFT JOIN Stock s ON s.idProducto = p.idProducto";

        List<string> condiciones = [];
        if (idCategoria.HasValue)
        {
            condiciones.Add("c.idCategoria = @idCategoria");
        }
        if (tieneTerm)
        {
            condiciones.Add("(p.nombre LIKE @like OR p.codigoBarras = @term OR p.codigoBarras LIKE @like)");
        }

        if (condiciones.Count > 0)
        {
            sql += " WHERE " + string.Join(" AND ", condiciones);
        }

        sql += @"
            GROUP BY p.idProducto, p.nombre, p.codigoBarras, c.nombreCategoria
            HAVING COALESCE(SUM(s.cantidad), 0) > 0
            ORDER BY cantidadDisponible DESC;";

        using SQLiteCommand command = new(sql, connection);
        if (idCategoria.HasValue)
        {
            command.Parameters.AddWithValue("@idCategoria", idCategoria.Value);
        }
        if (tieneTerm)
        {
            string normalized = term!.Trim();
            command.Parameters.AddWithValue("@like", $"%{normalized}%");
            command.Parameters.AddWithValue("@term", normalized);
        }

        using SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            disponibles.Add(new StockDisponible
            {
                IdProducto = Convert.ToInt32(reader["idProducto"]),
                NombreProducto = reader["nombre"].ToString() ?? string.Empty,
                CodigoBarras = reader["codigoBarras"].ToString() ?? string.Empty,
                NombreCategoria = reader["nombreCategoria"].ToString() ?? string.Empty,
                CantidadDisponible = Convert.ToInt32(reader["cantidadDisponible"])
            });
        }

        return disponibles;
    }
}
