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
    public DbSet<TipoHabitacion> TiposHabitacion { get; set; }
    public DbSet<Habitacion> Habitaciones { get; set; }

    // Pacientes
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<HistorialMedico> HistorialesMedicos { get; set; }
    public DbSet<ContactoEmergencia> ContactosEmergencia { get; set; }
    public DbSet<AsignacionHabitacion> AsignacionesHabitacion { get; set; }

    // Personal
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Especialidad> Especialidades { get; set; }

    // Historia cl├¡nica
    public DbSet<Consulta> Consultas { get; set; }
    public DbSet<Diagnostico> Diagnosticos { get; set; }
    public DbSet<Tratamiento> Tratamientos { get; set; }
    public DbSet<Examen> Examenes { get; set; }
    public DbSet<Evolucion> Evoluciones { get; set; }
    public DbSet<Receta> Recetas { get; set; }
    public DbSet<DetalleReceta> DetallesReceta { get; set; }

    // Farmacia e inventario
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Medicamento> Medicamentos { get; set; }
    public DbSet<LoteMedicamento> LotesMedicamento { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    // Ventas
    public DbSet<VentaFarmacia> VentasFarmacia { get; set; }
    public DbSet<DetalleVenta> DetallesVenta { get; set; }

    // Facturaci├│n
    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<DetalleFactura> DetallesFactura { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // =========================================================
        // HISTORIAL MEDICO
        // =========================================================

        modelBuilder.Entity<HistorialMedico>()
            .HasKey(x => x.IdHistorialMedico);

        modelBuilder.Entity<HistorialMedico>()
            .HasIndex(x => x.IdPaciente)
            .IsUnique();


        // =========================================================
        // CLAVES PRIMARIAS
        // =========================================================

        modelBuilder.Entity<Sucursal>()
            .HasKey(x => x.IdSucursal);

        modelBuilder.Entity<Habitacion>()
            .HasKey(x => x.IdHabitacion);

        modelBuilder.Entity<TipoHabitacion>()
            .HasKey(x => x.IdTipoHabitacion);

        modelBuilder.Entity<Paciente>()
            .HasKey(x => x.IdPaciente);

        modelBuilder.Entity<ContactoEmergencia>()
            .HasKey(x => x.IdContacto);

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

        modelBuilder.Entity<RolPermiso>()
            .HasIndex(x => new
            {
                x.IdRol,
                x.IdModulo,
                x.IdOperacion
            })
            .IsUnique();

        modelBuilder.Entity<Consulta>()
            .HasKey(x => x.IdConsulta);

        modelBuilder.Entity<Consulta>()
            .HasIndex(x => new
            {
                x.IdPaciente,
                x.NumeroConsultaPaciente
            })
            .IsUnique();

        modelBuilder.Entity<Diagnostico>()
            .HasKey(x => x.IdDiagnostico);

        modelBuilder.Entity<Tratamiento>()
            .HasKey(x => x.IdTratamiento);

        modelBuilder.Entity<Examen>()
            .HasKey(x => x.IdExamen);

        modelBuilder.Entity<Evolucion>()
            .HasKey(x => x.IdEvolucion);

        modelBuilder.Entity<Receta>()
            .HasKey(x => x.IdReceta);

        modelBuilder.Entity<DetalleReceta>()
            .HasKey(x => x.IdDetalleReceta);

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

        modelBuilder.Entity<Catalogo>()
            .HasKey(x => x.IdCatalogo);

        modelBuilder.Entity<Factura>()
            .HasKey(x => x.IdFactura);

        modelBuilder.Entity<DetalleFactura>()
            .HasKey(x => x.IdDetalleFactura);

        modelBuilder.Entity<Catalogo>()
            .ToTable("Catalogo");

        modelBuilder.Entity<Factura>()
            .ToTable("Facturas");

        modelBuilder.Entity<DetalleFactura>()
            .ToTable("DetalleFactura");

        // =========================================================
        // TIPO HABITACION -> HABITACIONES
        // =========================================================

        modelBuilder.Entity<Habitacion>()
            .HasOne(x => x.TipoHabitacion)
            .WithMany(x => x.Habitaciones)
            .HasForeignKey(x => x.IdTipoHabitacion)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // PACIENTE -> FACTURAS
        // =========================================================

        modelBuilder.Entity<Factura>()
            .HasOne(x => x.Paciente)
            .WithMany(x => x.Facturas)
            .HasForeignKey(x => x.IdPaciente)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // USUARIO -> FACTURAS
        // =========================================================

        modelBuilder.Entity<Factura>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.Facturas)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // VENTA DE FARMACIA -> FACTURA
        // Una venta puede tener como m├íximo una factura.
        // =========================================================

        modelBuilder.Entity<Factura>()
            .HasOne(x => x.Venta)
            .WithOne()
            .HasForeignKey<Factura>(x => x.IdVenta)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
            .HasIndex(x => x.IdVenta)
            .IsUnique()
            .HasFilter("[IdVenta] IS NOT NULL");

        // =========================================================
        // FACTURA -> DETALLES
        // =========================================================

        modelBuilder.Entity<DetalleFactura>()
            .HasOne(x => x.Factura)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdFactura)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================================================
        // CATALOGO -> DETALLES DE FACTURA
        // =========================================================

        modelBuilder.Entity<DetalleFactura>()
            .HasOne(x => x.Catalogo)
            .WithMany(x => x.DetallesFactura)
            .HasForeignKey(x => x.IdCatalogo)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // ASIGNACION HABITACION -> DETALLE FACTURA
        // =========================================================

        modelBuilder.Entity<DetalleFactura>()
            .HasOne(x => x.AsignacionHabitacion)
            .WithMany()
            .HasForeignKey(x => x.IdAsignacionHabitacion)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // MEDICAMENTO -> CATALOGO
        // =========================================================

        modelBuilder.Entity<Catalogo>()
            .HasOne(x => x.Medicamento)
            .WithMany()
            .HasForeignKey(x => x.IdMedicamento)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // TIPO HABITACION -> CATALOGO
        // =========================================================

        modelBuilder.Entity<Catalogo>()
            .HasOne(x => x.TipoHabitacion)
            .WithMany()
            .HasForeignKey(x => x.IdTipoHabitacion)
            .OnDelete(DeleteBehavior.Restrict);

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
        // TURNO -> EMPLEADOS
        // =========================================================

        modelBuilder.Entity<Empleado>()
            .HasOne(x => x.Turno)
            .WithMany(x => x.Empleados)
            .HasForeignKey(x => x.IdTurno)
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
        // PACIENTE -> HISTORIAL MEDICO
        // Un paciente tiene un unico historial.
        // El historial almacena solamente la entrevista inicial;
        // los eventos clinicos se agregan dinamicamente.
        // =========================================================

        modelBuilder.Entity<HistorialMedico>()
            .HasOne(x => x.Paciente)
            .WithOne(x => x.HistorialMedico)
            .HasForeignKey<HistorialMedico>(
                x => x.IdPaciente)
            .OnDelete(DeleteBehavior.Cascade);
        // =========================================================
        // PACIENTE -> CONTACTOS DE EMERGENCIA
        // =========================================================

        modelBuilder.Entity<ContactoEmergencia>()
            .ToTable("ContactosEmergencia");

        modelBuilder.Entity<ContactoEmergencia>()
            .HasOne(x => x.Paciente)
            .WithMany(x => x.ContactosEmergencia)
            .HasForeignKey(x => x.IdPaciente)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================================================
        // PACIENTE -> ASIGNACIONES
        // =========================================================

        modelBuilder.Entity<AsignacionHabitacion>()
            .HasOne(x => x.Paciente)
            .WithMany(x => x.AsignacionesHabitacion)
            .HasForeignKey(x => x.IdPaciente)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // DIAGNOSTICO -> HOSPITALIZACION
        // =========================================================

        modelBuilder.Entity<AsignacionHabitacion>()
            .HasOne(x => x.Diagnostico)
            .WithMany()
            .HasForeignKey(x => x.IdDiagnostico)
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
        // DIAGNOSTICO -> EXAMENES
        // =========================================================

        modelBuilder.Entity<Examen>()
            .HasOne(x => x.Diagnostico)
            .WithMany()
            .HasForeignKey(x => x.IdDiagnostico)
            .OnDelete(DeleteBehavior.Restrict);

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
        // RECETA -> EVOLUCIONES
        // =========================================================

        modelBuilder.Entity<Evolucion>()
            .HasOne(x => x.Receta)
            .WithMany()
            .HasForeignKey(x => x.IdReceta)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // TRATAMIENTO -> EVOLUCIONES
        // =========================================================

        modelBuilder.Entity<Evolucion>()
            .HasOne(x => x.Tratamiento)
            .WithMany()
            .HasForeignKey(x => x.IdTratamiento)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================================================
        // CONSULTA -> RECETAS
        // =========================================================

        modelBuilder.Entity<Receta>()
            .HasOne(x => x.Consulta)
            .WithMany(x => x.Recetas)
            .HasForeignKey(x => x.IdConsulta)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================================================
        // RECETA -> DETALLES
        // =========================================================

        modelBuilder.Entity<DetalleReceta>()
            .HasOne(x => x.Receta)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdReceta)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================================================
        // MEDICAMENTO -> DETALLES DE RECETA
        // =========================================================

        modelBuilder.Entity<DetalleReceta>()
            .HasOne(x => x.Medicamento)
            .WithMany(x => x.DetallesReceta)
            .HasForeignKey(x => x.IdMedicamento)
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

        modelBuilder.Entity<ContactoEmergencia>()
            .Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<ContactoEmergencia>()
            .Property(x => x.Parentesco)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<ContactoEmergencia>()
            .Property(x => x.Telefono)
            .HasMaxLength(30)
            .IsRequired();

        modelBuilder.Entity<ContactoEmergencia>()
            .Property(x => x.TelefonoAlternativo)
            .HasMaxLength(30);

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

        modelBuilder.Entity<Receta>()
            .Property(x => x.Observaciones)
            .HasMaxLength(500);

        modelBuilder.Entity<DetalleReceta>()
            .Property(x => x.Dosis)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<DetalleReceta>()
            .Property(x => x.Frecuencia)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<DetalleReceta>()
            .Property(x => x.Indicaciones)
            .HasMaxLength(500);

        modelBuilder.Entity<TipoHabitacion>()
            .Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<TipoHabitacion>()
            .Property(x => x.Descripcion)
            .HasMaxLength(250);

        modelBuilder.Entity<TipoHabitacion>()
            .Property(x => x.PrecioPorDia)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Catalogo>()
            .Property(x => x.Codigo)
            .HasMaxLength(30)
            .IsRequired();

        modelBuilder.Entity<Catalogo>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<Catalogo>()
            .Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Catalogo>()
            .Property(x => x.Descripcion)
            .HasMaxLength(500);

        modelBuilder.Entity<Catalogo>()
            .Property(x => x.Tipo)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<Catalogo>()
            .Property(x => x.Precio)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Factura>()
            .Property(x => x.NumeroFactura)
            .HasMaxLength(30)
            .IsRequired();

        modelBuilder.Entity<Factura>()
            .HasIndex(x => x.NumeroFactura)
            .IsUnique();

        modelBuilder.Entity<Factura>()
            .Property(x => x.Total)
            .HasPrecision(12, 2);

        modelBuilder.Entity<Factura>()
            .Property(x => x.Estado)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<DetalleFactura>()
            .Property(x => x.Descripcion)
            .HasMaxLength(250)
            .IsRequired();

        modelBuilder.Entity<DetalleFactura>()
            .Property(x => x.PrecioUnitario)
            .HasPrecision(10, 2);

        modelBuilder.Entity<DetalleFactura>()
            .Property(x => x.Total)
            .HasPrecision(12, 2);
    }
}
