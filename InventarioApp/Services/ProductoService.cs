using InventarioApp.Data;
using InventarioApp.Models;

namespace InventarioApp.Services;

public class ProductoService
{
    public List<Producto> ListarProductos(string? terminoBusqueda = null)
    {
        return string.IsNullOrWhiteSpace(terminoBusqueda)
            ? Database.GetProductos()
            : Database.BuscarProductos(terminoBusqueda);
    }

    public void RegistrarProducto(string nombre, string codigoBarras, decimal precio, string marca, int idCategoria)
    {
        ValidarCamposObligatorios(nombre, codigoBarras, idCategoria);
        ValidarPrecio(precio);

        if (Database.ExisteCodigoBarras(codigoBarras))
            throw new InvalidOperationException("El código de barras ya está registrado para otro producto.");

        if (!Database.ExisteCategoriaPorId(idCategoria))
            throw new InvalidOperationException("La categoria seleccionada no existe.");

        Database.InsertProducto(nombre, codigoBarras, precio, marca, idCategoria);
    }

    public void EditarProducto(int idProducto, string nombre, string codigoBarras, decimal precio, string marca, int idCategoria)
    {
        if (idProducto <= 0)
            throw new InvalidOperationException("IdProducto no válido.");

        if (!Database.ExisteProductoPorId(idProducto))
            throw new InvalidOperationException("El producto seleccionado no existe.");

        ValidarCamposObligatorios(nombre, codigoBarras, idCategoria);
        ValidarPrecio(precio);

        if (Database.ExisteCodigoBarras(codigoBarras, idProducto))
            throw new InvalidOperationException("El código de barras ya está registrado para otro producto.");

        if (!Database.ExisteCategoriaPorId(idCategoria))
            throw new InvalidOperationException("La categoria seleccionada no existe.");

        Database.UpdateProducto(idProducto, nombre, codigoBarras, precio, marca, idCategoria);
    }

    private static void ValidarCamposObligatorios(string nombre, string codigoBarras, int idCategoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException("El nombre del producto es obligatorio.");

        if (string.IsNullOrWhiteSpace(codigoBarras))
            throw new InvalidOperationException("El código de barras es obligatorio.");

        if (idCategoria <= 0)
            throw new InvalidOperationException("Debe seleccionar una categoria.");
    }

    private static void ValidarPrecio(decimal precio)
    {
        // Sugerencia de negocio: precio debe ser mayor o igual a 0, pero se valida como > 0.
        if (precio <= 0m)
            throw new InvalidOperationException("El precio debe ser mayor a 0.");
    }

    public class ProductoSeleccionProducto
    {
        public ProductoSeleccionProducto(int idProducto, string nombre, string codigoBarras)
        {
            IdProducto = idProducto;
            Nombre = nombre;
            CodigoBarras = codigoBarras;
        }

        public int IdProducto { get; }
        public string Nombre { get; }
        public string CodigoBarras { get; }

        public override string ToString() => $"{Nombre} ({CodigoBarras})";
    }
}

