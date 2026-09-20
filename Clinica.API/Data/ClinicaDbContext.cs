using Clinica.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Data;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options)
        : base(options)
    {
    }

    // Seguridad y usuarios
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Modulo> Modulos { get; set; }
    public DbSet<Operacion> Operaciones { get; set; }
    public DbSet<RolPermiso> RolPermisos { get; set; }

    // Sucursales y habitaciones
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<Habitacion> Habitaciones { get; set; }

    // Pacientes
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<AsignacionHabitacion> AsignacionesHabitacion { get; set; }

    // Personal
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Especialidad> Especialidades { get; set; }

    // Historia clínica
    public DbSet<Consulta> Consultas { get; set; }
    public DbSet<Diagnostico> Diagnosticos { get; set; }
    public DbSet<Tratamiento> Tratamientos { get; set; }
    public DbSet<Examen> Examenes { get; set; }
    public DbSet<Evolucion> Evoluciones { get; set; }

    // Farmacia e inventario
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Medicamento> Medicamentos { get; set; }
    public DbSet<LoteMedicamento> LotesMedicamento { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    // Ventas
    public DbSet<VentaFarmacia> VentasFarmacia { get; set; }
    public DbSet<DetalleVenta> DetallesVenta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================================================
        // CLAVES PRIMARIAS
        // =========================================================

        modelBuilder.Entity<Sucursal>()
            .HasKey(x => x.IdSucursal);

        modelBuilder.Entity<Habitacion>()
            .HasKey(x => x.IdHabitacion);

        modelBuilder.Entity<Paciente>()
            .HasKey(x => x.IdPaciente);

        modelBuilder.Entity<AsignacionHabitacion>()
            .HasKey(x => x.IdAsignacion);

        modelBuilder.Entity<Empleado>()
            .HasKey(x => x.IdEmpleado);

        modelBuilder.Entity<Especialidad>()
            .HasKey(x => x.IdEspecialidad);

        modelBuilder.Entity<Usuario>()
            .HasKey(x => x.IdUsuario);

        modelBuilder.Entity<Rol>()
            .HasKey(x => x.IdRol);

        modelBuilder.Entity<Modulo>()
            .HasKey(x => x.IdModulo);

        modelBuilder.Entity<Operacion>()
            .HasKey(x => x.IdOperacion);

        modelBuilder.Entity<RolPermiso>()
            .HasKey(x => x.IdRolPermiso);

        modelBuilder.Entity<Consulta>()
            .HasKey(x => x.IdConsulta);

        modelBuilder.Entity<Diagnostico>()
            .HasKey(x => x.IdDiagnostico);

        modelBuilder.Entity<Tratamiento>()
            .HasKey(x => x.IdTratamiento);

        modelBuilder.Entity<Examen>()
            .HasKey(x => x.IdExamen);

        modelBuilder.Entity<Evolucion>()
            .HasKey(x => x.IdEvolucion);

        modelBuilder.Entity<Categoria>()
            .HasKey(x => x.IdCategoria);

        modelBuilder.Entity<Marca>()
            .HasKey(x => x.IdMarca);

        modelBuilder.Entity<Medicamento>()
            .HasKey(x => x.IdMedicamento);

        modelBuilder.Entity<LoteMedicamento>()
            .HasKey(x => x.IdLote);

        modelBuilder.Entity<MovimientoInventario>()
            .HasKey(x => x.IdMovimiento);

        modelBuilder.Entity<VentaFarmacia>()
            .HasKey(x => x.IdVenta);

        modelBuilder.Entity<DetalleVenta>()
            .HasKey(x => x.IdDetalle);
        modelBuilder.Entity<DetalleVenta>()
            .ToTable("DetalleVenta");


        // =========================================================
        // SUCURSAL -> HABITACIONES
        // =========================================================

        modelBuilder.Entity<Habitacion>()
            .HasOne(x => x.Sucursal)
            .WithMany(x => x.Habitaciones)
            .HasForeignKey(x => x.IdSucursal)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // SUCURSAL -> EMPLEADOS
        // =========================================================

        modelBuilder.Entity<Empleado>()
            .HasOne(x => x.Sucursal)
            .WithMany(x => x.Empleados)
            .HasForeignKey(x => x.IdSucursal)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // ESPECIALIDAD -> EMPLEADOS
        // =========================================================

        modelBuilder.Entity<Empleado>()
            .HasOne(x => x.Especialidad)
            .WithMany(x => x.Empleados)
            .HasForeignKey(x => x.IdEspecialidad)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // PACIENTE -> ASIGNACIONES
        // =========================================================

        modelBuilder.Entity<AsignacionHabitacion>()
            .HasOne(x => x.Paciente)
            .WithMany(x => x.AsignacionesHabitacion)
            .HasForeignKey(x => x.IdPaciente)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // HABITACION -> ASIGNACIONES
        // =========================================================

        modelBuilder.Entity<AsignacionHabitacion>()
            .HasOne(x => x.Habitacion)
            .WithMany(x => x.Asignaciones)
            .HasForeignKey(x => x.IdHabitacion)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // EMPLEADO -> USUARIOS
        // =========================================================

        modelBuilder.Entity<Usuario>()
            .HasOne(x => x.Empleado)
            .WithMany(x => x.Usuarios)
            .HasForeignKey(x => x.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // ROL -> USUARIOS
        // =========================================================

        modelBuilder.Entity<Usuario>()
            .HasOne(x => x.Rol)
            .WithMany(x => x.Usuarios)
            .HasForeignKey(x => x.IdRol)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // ROL -> PERMISOS
        // =========================================================

        modelBuilder.Entity<RolPermiso>()
            .HasOne(x => x.Rol)
            .WithMany(x => x.RolPermisos)
            .HasForeignKey(x => x.IdRol)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // MODULO -> PERMISOS
        // =========================================================

        modelBuilder.Entity<RolPermiso>()
            .HasOne(x => x.Modulo)
            .WithMany(x => x.RolPermisos)
            .HasForeignKey(x => x.IdModulo)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // OPERACION -> PERMISOS
        // =========================================================

        modelBuilder.Entity<RolPermiso>()
            .HasOne(x => x.Operacion)
            .WithMany(x => x.RolPermisos)
            .HasForeignKey(x => x.IdOperacion)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // PACIENTE -> CONSULTAS
        // =========================================================

        modelBuilder.Entity<Consulta>()
            .HasOne(x => x.Paciente)
            .WithMany(x => x.Consultas)
            .HasForeignKey(x => x.IdPaciente)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // EMPLEADO -> CONSULTAS
        // =========================================================

        modelBuilder.Entity<Consulta>()
            .HasOne(x => x.Empleado)
            .WithMany(x => x.Consultas)
            .HasForeignKey(x => x.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // CONSULTA -> DIAGNOSTICOS
        // =========================================================

        modelBuilder.Entity<Diagnostico>()
            .HasOne(x => x.Consulta)
            .WithMany(x => x.Diagnosticos)
            .HasForeignKey(x => x.IdConsulta)
            .OnDelete(DeleteBehavior.Cascade);


        // =========================================================
        // CONSULTA -> TRATAMIENTOS
        // =========================================================

        modelBuilder.Entity<Tratamiento>()
            .HasOne(x => x.Consulta)
            .WithMany(x => x.Tratamientos)
            .HasForeignKey(x => x.IdConsulta)
            .OnDelete(DeleteBehavior.Cascade);


        // =========================================================
        // CONSULTA -> EXAMENES
        // =========================================================

        modelBuilder.Entity<Examen>()
            .HasOne(x => x.Consulta)
            .WithMany(x => x.Examenes)
            .HasForeignKey(x => x.IdConsulta)
            .OnDelete(DeleteBehavior.Cascade);


        // =========================================================
        // CONSULTA -> EVOLUCIONES
        // =========================================================

        modelBuilder.Entity<Evolucion>()
            .HasOne(x => x.Consulta)
            .WithMany(x => x.Evoluciones)
            .HasForeignKey(x => x.IdConsulta)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Evolucion>()
            .HasOne(x => x.Empleado)
            .WithMany(x => x.Evoluciones)
            .HasForeignKey(x => x.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // CATEGORIA -> MEDICAMENTOS
        // =========================================================

        modelBuilder.Entity<Medicamento>()
            .HasOne(x => x.Categoria)
            .WithMany(x => x.Medicamentos)
            .HasForeignKey(x => x.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // MARCA -> MEDICAMENTOS
        // =========================================================

        modelBuilder.Entity<Medicamento>()
            .HasOne(x => x.Marca)
            .WithMany(x => x.Medicamentos)
            .HasForeignKey(x => x.IdMarca)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // MEDICAMENTO -> LOTES
        // =========================================================

        modelBuilder.Entity<LoteMedicamento>()
            .HasOne(x => x.Medicamento)
            .WithMany(x => x.Lotes)
            .HasForeignKey(x => x.IdMedicamento)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // LOTE -> MOVIMIENTOS
        // =========================================================

        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(x => x.Lote)
            .WithMany(x => x.Movimientos)
            .HasForeignKey(x => x.IdLote)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // USUARIO -> MOVIMIENTOS
        // =========================================================

        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // USUARIO -> VENTAS
        // =========================================================

        modelBuilder.Entity<VentaFarmacia>()
            .HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // VENTA -> DETALLES
        // =========================================================

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(x => x.Venta)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdVenta)
            .OnDelete(DeleteBehavior.Cascade);


        // =========================================================
        // LOTE -> DETALLES DE VENTA
        // =========================================================

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(x => x.Lote)
            .WithMany(x => x.DetallesVenta)
            .HasForeignKey(x => x.IdLote)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================================================
        // LONGITUDES Y CAMPOS OBLIGATORIOS
        // =========================================================

        modelBuilder.Entity<Sucursal>()
            .Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Sucursal>()
            .Property(x => x.Direccion)
            .HasMaxLength(200);

        modelBuilder.Entity<Sucursal>()
            .Property(x => x.Telefono)
            .HasMaxLength(30);


        modelBuilder.Entity<Habitacion>()
            .Property(x => x.Numero)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<Habitacion>()
            .Property(x => x.Estado)
            .HasMaxLength(20)
            .IsRequired();


        modelBuilder.Entity<Paciente>()
            .Property(x => x.Nombres)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Paciente>()
            .Property(x => x.Apellidos)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Paciente>()
            .Property(x => x.DPI)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<Paciente>()
            .Property(x => x.Direccion)
            .HasMaxLength(200);

        modelBuilder.Entity<Paciente>()
            .Property(x => x.Telefono)
            .HasMaxLength(30);

        modelBuilder.Entity<Paciente>()
            .Property(x => x.Correo)
            .HasMaxLength(150);


        modelBuilder.Entity<Empleado>()
            .Property(x => x.Nombres)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Empleado>()
            .Property(x => x.Apellidos)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Empleado>()
            .Property(x => x.DPI)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<Empleado>()
            .Property(x => x.Telefono)
            .HasMaxLength(30);

        modelBuilder.Entity<Empleado>()
            .Property(x => x.Correo)
            .HasMaxLength(150);


        modelBuilder.Entity<Especialidad>()
            .Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();


        modelBuilder.Entity<Rol>()
            .Property(x => x.Nombre)
            .HasMaxLength(50)
            .IsRequired();


        modelBuilder.Entity<Usuario>()
            .Property(x => x.UsuarioLogin)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.UsuarioLogin)
            .IsUnique();


        modelBuilder.Entity<Modulo>()
            .Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();


        modelBuilder.Entity<Operacion>()
            .Property(x => x.Nombre)
            .HasMaxLength(50)
            .IsRequired();


        modelBuilder.Entity<Categoria>()
            .Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();


        modelBuilder.Entity<Marca>()
            .Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();


        modelBuilder.Entity<Medicamento>()
            .Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Medicamento>()
            .Property(x => x.PrecioVenta)
            .HasPrecision(10, 2);


        modelBuilder.Entity<LoteMedicamento>()
            .Property(x => x.NumeroLote)
            .HasMaxLength(50)
            .IsRequired();


        modelBuilder.Entity<MovimientoInventario>()
            .Property(x => x.TipoMovimiento)
            .HasMaxLength(30)
            .IsRequired();


        modelBuilder.Entity<VentaFarmacia>()
            .Property(x => x.Total)
            .HasPrecision(10, 2);


        modelBuilder.Entity<DetalleVenta>()
            .Property(x => x.PrecioUnitario)
            .HasPrecision(10, 2);

        modelBuilder.Entity<DetalleVenta>()
            .Property(x => x.Total)
            .HasPrecision(10, 2);
    }
}