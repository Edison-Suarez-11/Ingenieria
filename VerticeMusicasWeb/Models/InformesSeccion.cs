namespace VerticeMusicasWeb.Models;

/// <summary>Identificadores de seccion para exportar/imprimir un informe individual.</summary>
public static class InformesSeccion
{
    public const string Ventas = "ventas";
    public const string MasVendidos = "mas-vendidos";
    public const string Salidas = "salidas";
    public const string Entradas = "entradas";
    public const string ComprasProveedor = "compras-proveedor";
    public const string Comparacion = "comparacion";
    public const string Proveedores = "proveedores";
    public const string Productos = "productos";
    public const string Categorias = "categorias";
    public const string Completo = "completo";

    private static readonly Dictionary<string, string> Titulos = new(StringComparer.OrdinalIgnoreCase)
    {
        [Ventas] = "Ventas — listado",
        [MasVendidos] = "Productos mas vendidos",
        [Salidas] = "Salidas de inventario",
        [Entradas] = "Entradas de inventario",
        [ComprasProveedor] = "Compras por proveedor",
        [Comparacion] = "Comparacion de precios",
        [Proveedores] = "Directorio de proveedores",
        [Productos] = "Catalogo de productos",
        [Categorias] = "Categorias",
        [Completo] = "Informe completo"
    };

    public static IReadOnlyCollection<string> Individuales { get; } =
        Titulos.Keys.Where(k => k != Completo).ToList();

    /// <summary>Normaliza la seccion; si falta o es invalida, usa ventas (nunca completo por defecto).</summary>
    public static string Normalizar(string? seccion)
    {
        if (string.IsNullOrWhiteSpace(seccion))
        {
            return Ventas;
        }

        string? key = Titulos.Keys.FirstOrDefault(k =>
            string.Equals(k, seccion.Trim(), StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrEmpty(key) ? Ventas : key;
    }

    public static bool EsCompleto(string seccion) =>
        string.Equals(seccion, Completo, StringComparison.OrdinalIgnoreCase);

    public static string ObtenerTitulo(string seccion) =>
        Titulos.TryGetValue(seccion, out string? titulo) ? titulo : "Informe";
}
