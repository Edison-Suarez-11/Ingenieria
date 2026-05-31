using System.ComponentModel.DataAnnotations;
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
                EF.Functions.Like(p.Contacto, like) ||
                (p.PersonaContacto != null && EF.Functions.Like(p.PersonaContacto, like)) ||
                (p.Celular != null && EF.Functions.Like(p.Celular, like)) ||
                (p.CorreoElectronico != null && EF.Functions.Like(p.CorreoElectronico, like)) ||
                (p.Ciudad != null && EF.Functions.Like(p.Ciudad, like)) ||
                (p.Direccion != null && EF.Functions.Like(p.Direccion, like)) ||
                (p.Nit != null && EF.Functions.Like(p.Nit, like)) ||
                (p.TelefonoFijo != null && EF.Functions.Like(p.TelefonoFijo, like)));
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
            var nuevo = MapearProveedor(proveedor);
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
            AplicarCambios(existente, proveedor);
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

    private static Proveedor MapearProveedor(Proveedor proveedor)
    {
        var nuevo = new Proveedor
        {
            Nombre = proveedor.Nombre.Trim(),
            PersonaContacto = NormalizarOpcional(proveedor.PersonaContacto),
            Celular = NormalizarOpcional(proveedor.Celular),
            CorreoElectronico = NormalizarOpcional(proveedor.CorreoElectronico)?.ToLowerInvariant(),
            Ciudad = NormalizarOpcional(proveedor.Ciudad),
            Direccion = NormalizarOpcional(proveedor.Direccion),
            Nit = NormalizarOpcional(proveedor.Nit),
            TelefonoFijo = NormalizarOpcional(proveedor.TelefonoFijo)
        };
        nuevo.Contacto = ConstruirContactoLegacy(nuevo);
        return nuevo;
    }

    private static void AplicarCambios(Proveedor existente, Proveedor proveedor)
    {
        existente.Nombre = proveedor.Nombre.Trim();
        existente.PersonaContacto = NormalizarOpcional(proveedor.PersonaContacto);
        existente.Celular = NormalizarOpcional(proveedor.Celular);
        existente.CorreoElectronico = NormalizarOpcional(proveedor.CorreoElectronico)?.ToLowerInvariant();
        existente.Ciudad = NormalizarOpcional(proveedor.Ciudad);
        existente.Direccion = NormalizarOpcional(proveedor.Direccion);
        existente.Nit = NormalizarOpcional(proveedor.Nit);
        existente.TelefonoFijo = NormalizarOpcional(proveedor.TelefonoFijo);
        existente.Contacto = ConstruirContactoLegacy(existente);
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string ConstruirContactoLegacy(Proveedor proveedor)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(proveedor.Celular)) partes.Add(proveedor.Celular);
        if (!string.IsNullOrWhiteSpace(proveedor.CorreoElectronico)) partes.Add(proveedor.CorreoElectronico);
        if (partes.Count > 0) return string.Join(" / ", partes);
        return proveedor.Contacto?.Trim() ?? string.Empty;
    }

    private static ProveedorOperacionResultado ValidarCampos(Proveedor proveedor)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(proveedor.Nombre))
        {
            errores[nameof(Proveedor.Nombre)] = ["El nombre del proveedor es obligatorio."];
        }

        bool tieneCelular = !string.IsNullOrWhiteSpace(proveedor.Celular);
        bool tieneCorreo = !string.IsNullOrWhiteSpace(proveedor.CorreoElectronico);
        if (!tieneCelular && !tieneCorreo)
        {
            errores[nameof(Proveedor.Celular)] = ["Indica al menos un celular o un correo electronico."];
            errores[nameof(Proveedor.CorreoElectronico)] = ["Indica al menos un celular o un correo electronico."];
        }

        if (tieneCorreo && !new EmailAddressAttribute().IsValid(proveedor.CorreoElectronico))
        {
            errores[nameof(Proveedor.CorreoElectronico)] = ["El correo electronico no tiene un formato valido."];
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
