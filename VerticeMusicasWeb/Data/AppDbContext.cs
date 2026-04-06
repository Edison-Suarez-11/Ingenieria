using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categoria");
            entity.HasKey(c => c.IdCategoria);
            entity.Property(c => c.IdCategoria)
                .HasColumnName("idCategoria")
                .ValueGeneratedOnAdd();
            entity.Property(c => c.NombreCategoria)
                .HasColumnName("nombreCategoria")
                .IsRequired();
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Producto");
            entity.HasKey(p => p.IdProducto);
            entity.Property(p => p.IdProducto)
                .HasColumnName("idProducto")
                .ValueGeneratedOnAdd();
            entity.Property(p => p.Nombre)
                .HasColumnName("nombre")
                .IsRequired();
            entity.Property(p => p.CodigoBarras)
                .HasColumnName("codigoBarras")
                .IsRequired();
            entity.Property(p => p.Precio)
                .HasColumnName("precio");
            entity.Property(p => p.Marca)
                .HasColumnName("marca");
            entity.Property(p => p.IdCategoria)
                .HasColumnName("idCategoria");

            entity.HasIndex(p => p.CodigoBarras).IsUnique();

            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.ToTable("Inventario");
            entity.HasKey(i => i.IdInventario);
            entity.Property(i => i.IdInventario)
                .HasColumnName("idInventario")
                .ValueGeneratedOnAdd();
            entity.Property(i => i.Fecha)
                .HasColumnName("fecha")
                .IsRequired();
        });

        modelBuilder.Entity<MovimientoStock>(entity =>
        {
            entity.ToTable("Stock");
            entity.HasKey(m => m.IdStock);
            entity.Property(m => m.IdStock)
                .HasColumnName("idStock")
                .ValueGeneratedOnAdd();
            entity.Property(m => m.Cantidad)
                .HasColumnName("cantidad")
                .IsRequired();
            entity.Property(m => m.IdInventario)
                .HasColumnName("idInventario");
            entity.Property(m => m.IdProducto)
                .HasColumnName("idProducto");

            entity.HasOne(m => m.Inventario)
                .WithMany(i => i.Movimientos)
                .HasForeignKey(m => m.IdInventario)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Producto)
                .WithMany(p => p.MovimientosStock)
                .HasForeignKey(m => m.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => m.IdProducto);
            entity.HasIndex(m => m.IdInventario);
        });
    }
}
