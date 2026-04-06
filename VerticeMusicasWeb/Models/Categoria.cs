using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoria es obligatorio.")]
    [Display(Name = "Nombre de Categoria")]
    public string NombreCategoria { get; set; } = string.Empty;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
