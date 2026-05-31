using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class Proveedor
{
    [Key]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Persona de contacto")]
    public string? PersonaContacto { get; set; }

    [Display(Name = "Celular")]
    [Phone(ErrorMessage = "El celular no tiene un formato valido.")]
    public string? Celular { get; set; }

    [Display(Name = "Correo electronico")]
    [EmailAddress(ErrorMessage = "El correo electronico no tiene un formato valido.")]
    public string? CorreoElectronico { get; set; }

    [Display(Name = "Ciudad")]
    public string? Ciudad { get; set; }

    [Display(Name = "Direccion")]
    public string? Direccion { get; set; }

    [Display(Name = "NIT")]
    public string? Nit { get; set; }

    [Display(Name = "Telefono fijo")]
    public string? TelefonoFijo { get; set; }

    [Display(Name = "Contacto")]
    public string Contacto { get; set; } = string.Empty;

    public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();

    public string ResumenContacto()
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(Celular)) partes.Add(Celular.Trim());
        if (!string.IsNullOrWhiteSpace(CorreoElectronico)) partes.Add(CorreoElectronico.Trim());
        if (partes.Count > 0) return string.Join(" · ", partes);
        return string.IsNullOrWhiteSpace(Contacto) ? "—" : Contacto.Trim();
    }
}
