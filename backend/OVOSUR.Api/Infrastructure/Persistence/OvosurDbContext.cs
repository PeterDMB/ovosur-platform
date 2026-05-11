using Microsoft.EntityFrameworkCore;
using OVOSUR.Api.Modules.Employees.Entities;
using OVOSUR.Api.Modules.Security.Entities;
using OVOSUR.Api.Modules.Suppliers.Entities;

namespace OVOSUR.Api.Infrastructure.Persistence;

public sealed class OvosurDbContext(DbContextOptions<OvosurDbContext> options) : DbContext(options)
{
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Submodulo> Submodulos => Set<Submodulo>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<ProveedorContacto> ProveedorContactos => Set<ProveedorContacto>();
    public DbSet<ProveedorDocumento> ProveedorDocumentos => Set<ProveedorDocumento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        ConfigureSecurity(modelBuilder);
        ConfigureEmployees(modelBuilder);
        ConfigureSuppliers(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Modulo>(entity =>
        {
            entity.ToTable("Modulos", "Seguridad");
            entity.HasKey(x => x.ModuloId);
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.Property(x => x.Codigo).HasMaxLength(50);
            entity.Property(x => x.Nombre).HasMaxLength(120);
            entity.Property(x => x.Descripcion).HasMaxLength(300);
        });

        modelBuilder.Entity<Submodulo>(entity =>
        {
            entity.ToTable("Submodulos", "Seguridad");
            entity.HasKey(x => x.SubmoduloId);
            entity.HasIndex(x => new { x.ModuloId, x.Codigo }).IsUnique();
            entity.Property(x => x.Codigo).HasMaxLength(50);
            entity.Property(x => x.Nombre).HasMaxLength(120);
            entity.Property(x => x.Ruta).HasMaxLength(180);
            entity.HasOne(x => x.Modulo)
                .WithMany(x => x.Submodulos)
                .HasForeignKey(x => x.ModuloId);
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("Permisos", "Seguridad");
            entity.HasKey(x => x.PermisoId);
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.Property(x => x.Codigo).HasMaxLength(40);
            entity.Property(x => x.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles", "Seguridad");
            entity.HasKey(x => x.RolId);
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.Property(x => x.Codigo).HasMaxLength(50);
            entity.Property(x => x.Nombre).HasMaxLength(120);
            entity.Property(x => x.Descripcion).HasMaxLength(300);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios", "Seguridad");
            entity.HasKey(x => x.UsuarioId);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.PasswordHash).HasMaxLength(500);
            entity.Property(x => x.TipoUsuario).HasMaxLength(20);
            entity.Property(x => x.EstadoAprobacion).HasMaxLength(20);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("UsuarioRoles", "Seguridad");
            entity.HasKey(x => new { x.UsuarioId, x.RolId });
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.UsuarioId);
            entity.HasOne(x => x.Rol)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.RolId);
        });

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.ToTable("RolPermisos", "Seguridad");
            entity.HasKey(x => new { x.RolId, x.SubmoduloId, x.PermisoId });
            entity.HasOne(x => x.Rol)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.RolId);
            entity.HasOne(x => x.Submodulo)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.SubmoduloId);
            entity.HasOne(x => x.Permiso)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.PermisoId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", "Seguridad");
            entity.HasKey(x => x.RefreshTokenId);
            entity.Property(x => x.TokenHash).HasMaxLength(300);
            entity.Property(x => x.IpCreacion).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(300);
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<AuditoriaEvento>(entity =>
        {
            entity.ToTable("AuditoriaEventos", "Seguridad");
            entity.HasKey(x => x.AuditoriaEventoId);
            entity.Property(x => x.Evento).HasMaxLength(120);
            entity.Property(x => x.Entidad).HasMaxLength(120);
            entity.Property(x => x.EntidadId).HasMaxLength(80);
            entity.Property(x => x.Ip).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(300);
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.AuditoriaEventos)
                .HasForeignKey(x => x.UsuarioId);
        });
    }

    private static void ConfigureEmployees(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("Empleados", "Core");
            entity.HasKey(x => x.EmpleadoId);
            entity.HasIndex(x => x.UsuarioId).IsUnique();
            entity.Property(x => x.CodigoEmpleado).HasMaxLength(30);
            entity.Property(x => x.Nombres).HasMaxLength(120);
            entity.Property(x => x.Apellidos).HasMaxLength(120);
            entity.Property(x => x.Area).HasMaxLength(80);
            entity.Property(x => x.Cargo).HasMaxLength(100);
            entity.HasOne(x => x.Usuario)
                .WithOne(x => x.Empleado)
                .HasForeignKey<Empleado>(x => x.UsuarioId);
        });
    }

    private static void ConfigureSuppliers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.ToTable("Proveedores", "Proveedores");
            entity.HasKey(x => x.ProveedorId);
            entity.HasIndex(x => x.UsuarioId).IsUnique();
            entity.HasIndex(x => x.Ruc).IsUnique();
            entity.Property(x => x.Ruc).HasMaxLength(11).IsFixedLength();
            entity.Property(x => x.RazonSocial).HasMaxLength(200);
            entity.Property(x => x.NombreComercial).HasMaxLength(200);
            entity.Property(x => x.DireccionFiscal).HasMaxLength(250);
            entity.Property(x => x.Telefono).HasMaxLength(40);
            entity.Property(x => x.EstadoHomologacion).HasMaxLength(20);
            entity.HasOne(x => x.Usuario)
                .WithOne(x => x.Proveedor)
                .HasForeignKey<Proveedor>(x => x.UsuarioId);
        });

        modelBuilder.Entity<ProveedorContacto>(entity =>
        {
            entity.ToTable("ProveedorContactos", "Proveedores");
            entity.HasKey(x => x.ProveedorContactoId);
            entity.Property(x => x.TipoContacto).HasMaxLength(30);
            entity.Property(x => x.Nombres).HasMaxLength(120);
            entity.Property(x => x.Apellidos).HasMaxLength(120);
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.Telefono).HasMaxLength(40);
            entity.HasOne(x => x.Proveedor)
                .WithMany(x => x.Contactos)
                .HasForeignKey(x => x.ProveedorId);
        });

        modelBuilder.Entity<ProveedorDocumento>(entity =>
        {
            entity.ToTable("ProveedorDocumentos", "Proveedores");
            entity.HasKey(x => x.ProveedorDocumentoId);
            entity.Property(x => x.TipoDocumento).HasMaxLength(60);
            entity.Property(x => x.NombreArchivoOriginal).HasMaxLength(260);
            entity.Property(x => x.RutaFisica).HasMaxLength(500);
            entity.Property(x => x.MimeType).HasMaxLength(120);
            entity.Property(x => x.EstadoRevision).HasMaxLength(20);
            entity.HasOne(x => x.Proveedor)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.ProveedorId);
        });
    }
}
