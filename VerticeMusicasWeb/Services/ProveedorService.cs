using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class ProveedorService(AppDbContext context)
{
    public async Task<List<Proveedor>> ListarAsync(string? busqueda = null)
    {
        IQueryable<Proveedor> query = context.Proveedores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            string like = $"%{busqueda.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, like) ||
                EF.Functions.Like(p.Contacto, like));
        }

        return await query.OrderByDescending(p => p.IdProveedor).ToListAsync();
    }

    public async Task<Proveedor?> ObtenerPorIdAsync(int id)
    {
        return await context.Proveedores.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProveedor == id);
    }

    public async Task<ProveedorOperacionResultado> CrearAsync(Proveedor proveedor)
    {
        ProveedorOperacionResultado validacion = ValidarCampos(proveedor);
        if (!validacion.Exito)
        {
            return validacion;
        }

        if (await ExisteNombreAsync(proveedor.Nombre, null))
        {
            return ErrorCampo(nameof(Proveedor.Nombre), "Ya existe un proveedor con ese nombre.");
        }

        try
        {
            var nuevo = new Proveedor
            {
                Nombre = proveedor.Nombre.Trim(),
                Contacto = proveedor.Contacto.Trim()
            };

            context.Proveedores.Add(nuevo);
            await context.SaveChangesAsync();

            return new ProveedorOperacionResultado
            {
                Exito = true,
                Mensaje = "Proveedor registrado correctamente.",
                Proveedor = nuevo
            };
        }
        catch (Exception)
        {
            return new ProveedorOperacionResultado
            {
                Exito = false,
                Mensaje = "No se pudo registrar el proveedor."
            };
        }
    }

    public async Task<ProveedorOperacionResultado> ActualizarAsync(Proveedor proveedor)
    {
        ProveedorOperacionResultado validacion = ValidarCampos(proveedor);
        if (!validacion.Exito)
        {
            return validacion;
        }

        Proveedor? existente = await context.Proveedores
            .FirstOrDefaultAsync(p => p.IdProveedor == proveedor.IdProveedor);

        if (existente is null)
        {
            return new ProveedorOperacionResultado
            {
                Exito = false,
                Mensaje = "Proveedor no encontrado."
            };
        }

        if (await ExisteNombreAsync(proveedor.Nombre, proveedor.IdProveedor))
        {
            return ErrorCampo(nameof(Proveedor.Nombre), "Ya existe un proveedor con ese nombre.");
        }

        try
        {
            existente.Nombre = proveedor.Nombre.Trim();
            existente.Contacto = proveedor.Contacto.Trim();
            await context.SaveChangesAsync();

            return new ProveedorOperacionResultado
            {
                Exito = true,
                Mensaje = "Proveedor actualizado correctamente.",
                Proveedor = existente
            };
        }
        catch (Exception)
        {
            return new ProveedorOperacionResultado
            {
                Exito = false,
                Mensaje = "No se pudo actualizar el proveedor."
            };
        }
    }

    private static ProveedorOperacionResultado ValidarCampos(Proveedor proveedor)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(proveedor.Nombre))
        {
            errores[nameof(Proveedor.Nombre)] = ["El nombre del proveedor es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(proveedor.Contacto))
        {
            errores[nameof(Proveedor.Contacto)] = ["El contacto del proveedor es obligatorio."];
        }

        if (errores.Count > 0)
        {
            return new ProveedorOperacionResultado
            {
                Exito = false,
                Mensaje = "Revise los campos obligatorios.",
                Errores = errores
            };
        }

        return new ProveedorOperacionResultado { Exito = true };
    }

    private static ProveedorOperacionResultado ErrorCampo(string campo, string mensaje) =>
        new()
        {
            Exito = false,
            Mensaje = mensaje,
            Errores = new Dictionary<string, string[]> { [campo] = [mensaje] }
        };

    private async Task<bool> ExisteNombreAsync(string nombre, int? excluirId)
    {
        string n = nombre.Trim().ToLowerInvariant();
        return await context.Proveedores.AnyAsync(p =>
            p.Nombre.ToLower() == n &&
            (!excluirId.HasValue || p.IdProveedor != excluirId.Value));
    }
}
