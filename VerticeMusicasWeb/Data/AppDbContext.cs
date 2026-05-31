using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

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

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.ToTable("Proveedor");
            entity.HasKey(p => p.IdProveedor);
            entity.Property(p => p.IdProveedor)
                .HasColumnName("idProveedor")
                .ValueGeneratedOnAdd();
            entity.Property(p => p.Nombre)
                .HasColumnName("nombre")
                .IsRequired();
            entity.Property(p => p.PersonaContacto)
                .HasColumnName("personaContacto");
            entity.Property(p => p.Celular)
                .HasColumnName("celular");
            entity.Property(p => p.CorreoElectronico)
                .HasColumnName("correoElectronico");
            entity.Property(p => p.Ciudad)
                .HasColumnName("ciudad");
            entity.Property(p => p.Direccion)
                .HasColumnName("direccion");
            entity.Property(p => p.Nit)
                .HasColumnName("nit");
            entity.Property(p => p.TelefonoFijo)
                .HasColumnName("telefonoFijo");
            entity.Property(p => p.Contacto)
                .HasColumnName("contacto")
                .IsRequired();

            entity.HasIndex(p => p.Nombre).IsUnique();
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
            entity.Property(p => p.ManejaStock)
                .HasColumnName("manejaStock")
                .IsRequired();
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
            entity.Property(m => m.StockMinimo)
                .HasColumnName("stockMinimo")
                .IsRequired();
            entity.Property(m => m.IdInventario)
                .HasColumnName("idInventario");
            entity.Property(m => m.IdProducto)
                .HasColumnName("idProducto");
            entity.Property(m => m.IdProveedor)
                .HasColumnName("idProveedor");
            entity.Property(m => m.PrecioUnitarioCompra)
                .HasColumnName("precioUnitarioCompra");
            entity.Property(m => m.PorcentajeMargenVenta)
                .HasColumnName("porcentajeMargenVenta");
            entity.Property(m => m.PrecioVentaSugerido)
                .HasColumnName("precioVentaSugerido");

            entity.HasOne(m => m.Proveedor)
                .WithMany(p => p.MovimientosStock)
                .HasForeignKey(m => m.IdProveedor)
                .OnDelete(DeleteBehavior.SetNull);

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

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("Venta");
            entity.HasKey(v => v.IdVenta);
            entity.Property(v => v.IdVenta)
                .HasColumnName("idVenta")
                .ValueGeneratedOnAdd();
            entity.Property(v => v.Fecha)
                .HasColumnName("fecha")
                .IsRequired();
            entity.Property(v => v.Total)
                .HasColumnName("total")
                .IsRequired();
            entity.Property(v => v.MetodoPago)
                .HasColumnName("metodoPago")
                .IsRequired();
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable("DetalleVenta");
            entity.HasKey(d => d.IdDetalle);
            entity.Property(d => d.IdDetalle)
                .HasColumnName("idDetalle")
                .ValueGeneratedOnAdd();
            entity.Property(d => d.IdVenta)
                .HasColumnName("idVenta");
            entity.Property(d => d.IdProducto)
                .HasColumnName("idProducto");
            entity.Property(d => d.Cantidad)
                .HasColumnName("cantidad")
                .IsRequired();
            entity.Property(d => d.PrecioUnitario)
                .HasColumnName("precioUnitario")
                .IsRequired();

            entity.HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(d => d.IdVenta);
            entity.HasIndex(d => d.IdProducto);
        });
    }
}
