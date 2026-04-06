using InventarioApp.Data;
using InventarioApp.Models;

namespace InventarioApp.Services;

public class CategoriaService
{
    public List<Categoria> ListarCategorias(string? terminoBusqueda = null)
    {
        return string.IsNullOrWhiteSpace(terminoBusqueda)
            ? Database.GetCategorias()
            : Database.BuscarCategorias(terminoBusqueda);
    }

    public void RegistrarCategoria(string nombreCategoria)
    {
        if (string.IsNullOrWhiteSpace(nombreCategoria))
            throw new InvalidOperationException("El nombre de la categoria es obligatorio.");

        if (Database.ExisteCategoriaPorNombre(nombreCategoria))
            throw new InvalidOperationException("Ya existe una categoria con ese nombre.");

        Database.InsertCategoria(nombreCategoria);
    }

    public void EditarCategoria(int idCategoria, string nombreCategoria)
    {
        if (idCategoria <= 0)
            throw new InvalidOperationException("IdCategoria no válido.");

        if (string.IsNullOrWhiteSpace(nombreCategoria))
            throw new InvalidOperationException("El nombre de la categoria es obligatorio.");

        if (Database.ExisteCategoriaPorNombre(nombreCategoria, idCategoria))
            throw new InvalidOperationException("Ya existe una categoria con ese nombre.");

        if (!Database.ExisteCategoriaPorId(idCategoria))
            throw new InvalidOperationException("La categoria seleccionada no existe.");

        Database.UpdateCategoria(idCategoria, nombreCategoria);
    }
}

