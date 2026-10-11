using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Clinica.API.Data;
using Microsoft.EntityFrameworkCore;
using Clinica.API.Services;
using Clinica.API.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PacientesConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Pacientes", "Consultar"));
    });

    options.AddPolicy("PacientesCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Pacientes", "Crear"));
    });

    options.AddPolicy("PacientesModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Pacientes", "Modificar"));
    });

    options.AddPolicy("PacientesEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Pacientes", "Eliminar"));
    });

    options.AddPolicy("EmpleadosConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Empleados", "Consultar"));
    });

    options.AddPolicy("EmpleadosCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Empleados", "Crear"));
    });

    options.AddPolicy("EmpleadosModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Empleados", "Modificar"));
    });

    options.AddPolicy("EmpleadosEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Empleados", "Eliminar"));

	
	    options.AddPolicy("ConsultasConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Consultas", "Consultar"));
    });

    options.AddPolicy("ConsultasCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Consultas", "Crear"));
    });

    options.AddPolicy("ConsultasModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Consultas", "Modificar"));
    });

    options.AddPolicy("ConsultasEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Consultas", "Eliminar"));
    });
	
    options.AddPolicy("DiagnosticosConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Diagnosticos", "Consultar"));
    });

    options.AddPolicy("DiagnosticosCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Diagnosticos", "Crear"));
    });

    options.AddPolicy("DiagnosticosModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Diagnosticos", "Modificar"));
    });

    options.AddPolicy("DiagnosticosEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Diagnosticos", "Eliminar"));
    });

	    options.AddPolicy("TratamientosConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Tratamientos", "Consultar"));
    });

    options.AddPolicy("TratamientosCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Tratamientos", "Crear"));
    });

    options.AddPolicy("TratamientosModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Tratamientos", "Modificar"));
    });

    options.AddPolicy("TratamientosEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Tratamientos", "Eliminar"));
	    });
    options.AddPolicy("ExamenesConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Examenes", "Consultar"));
    });

    options.AddPolicy("ExamenesCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Examenes", "Crear"));
    });

    options.AddPolicy("ExamenesModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Examenes", "Modificar"));
    });

    options.AddPolicy("ExamenesEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Examenes", "Eliminar"));
    });

    options.AddPolicy("EvolucionesConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Evoluciones", "Consultar"));
    });

    options.AddPolicy("EvolucionesCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Evoluciones", "Crear"));
    });

    options.AddPolicy("EvolucionesModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Evoluciones", "Modificar"));
    });

    options.AddPolicy("EvolucionesEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Evoluciones", "Eliminar"));
    });
    options.AddPolicy("HabitacionesConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Habitaciones", "Consultar"));
    });

    options.AddPolicy("HabitacionesCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Habitaciones", "Crear"));
    });

    options.AddPolicy("HabitacionesModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Habitaciones", "Modificar"));
    });

    options.AddPolicy("HabitacionesEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Habitaciones", "Eliminar"));
    });

    options.AddPolicy("AsignacionesHabitacionConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("AsignacionesHabitacion", "Consultar"));
    });

    options.AddPolicy("AsignacionesHabitacionCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("AsignacionesHabitacion", "Crear"));
    });

    options.AddPolicy("AsignacionesHabitacionModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("AsignacionesHabitacion", "Modificar"));
    });

    options.AddPolicy("AsignacionesHabitacionEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("AsignacionesHabitacion", "Eliminar"));
    });
    options.AddPolicy("CategoriasConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Categorias", "Consultar"));
    });

    options.AddPolicy("CategoriasCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Categorias", "Crear"));
    });

    options.AddPolicy("CategoriasModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Categorias", "Modificar"));
    });

    options.AddPolicy("CategoriasEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Categorias", "Eliminar"));
    });

    options.AddPolicy("MarcasConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Marcas", "Consultar"));
    });

    options.AddPolicy("MarcasCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Marcas", "Crear"));
    });

    options.AddPolicy("MarcasModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Marcas", "Modificar"));
    });

    options.AddPolicy("MarcasEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Marcas", "Eliminar"));
    });
    options.AddPolicy("MedicamentosConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Medicamentos", "Consultar"));
    });

    options.AddPolicy("MedicamentosCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Medicamentos", "Crear"));
    });

    options.AddPolicy("MedicamentosModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Medicamentos", "Modificar"));
    });

    options.AddPolicy("MedicamentosEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Medicamentos", "Eliminar"));
    });

    options.AddPolicy("LotesMedicamentoConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("LotesMedicamento", "Consultar"));
    });

    options.AddPolicy("LotesMedicamentoCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("LotesMedicamento", "Crear"));
    });

    options.AddPolicy("LotesMedicamentoModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("LotesMedicamento", "Modificar"));
    });

    options.AddPolicy("LotesMedicamentoEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("LotesMedicamento", "Eliminar"));
    });
    options.AddPolicy("MovimientosInventarioConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("MovimientosInventario", "Consultar"));
    });

    options.AddPolicy("MovimientosInventarioCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("MovimientosInventario", "Crear"));
    });

    options.AddPolicy("MovimientosInventarioEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("MovimientosInventario", "Eliminar"));
    });
    options.AddPolicy("VentasFarmaciaConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("VentasFarmacia", "Consultar"));
    });

    options.AddPolicy("VentasFarmaciaCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("VentasFarmacia", "Crear"));
    });

    options.AddPolicy("VentasFarmaciaModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("VentasFarmacia", "Modificar"));
    });

    options.AddPolicy("VentasFarmaciaEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("VentasFarmacia", "Eliminar"));
    });
    options.AddPolicy("DetalleVentaConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleVenta", "Consultar"));
    });

    options.AddPolicy("DetalleVentaCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleVenta", "Crear"));
    });

    options.AddPolicy("DetalleVentaModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleVenta", "Modificar"));
    });

    options.AddPolicy("DetalleVentaEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleVenta", "Eliminar"));
    });
        options.AddPolicy("ContactosEmergenciaConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("ContactosEmergencia", "Consultar"));
    });

    options.AddPolicy("ContactosEmergenciaCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("ContactosEmergencia", "Crear"));
    });

    options.AddPolicy("ContactosEmergenciaModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("ContactosEmergencia", "Modificar"));
    });

    options.AddPolicy("ContactosEmergenciaEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("ContactosEmergencia", "Eliminar"));
    });
    options.AddPolicy("RecetasConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Recetas", "Consultar"));
    });

    options.AddPolicy("RecetasCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Recetas", "Crear"));
    });

    options.AddPolicy("RecetasModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Recetas", "Modificar"));
    });

    options.AddPolicy("RecetasEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Recetas", "Eliminar"));
    });

    options.AddPolicy("DetalleRecetaConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleReceta", "Consultar"));
    });

    options.AddPolicy("DetalleRecetaCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleReceta", "Crear"));
    });

    options.AddPolicy("DetalleRecetaModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleReceta", "Modificar"));
    });

    options.AddPolicy("DetalleRecetaEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleReceta", "Eliminar"));
    });
    options.AddPolicy("TiposHabitacionConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("TiposHabitacion", "Consultar"));
    });

    options.AddPolicy("TiposHabitacionCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("TiposHabitacion", "Crear"));
    });

    options.AddPolicy("TiposHabitacionModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("TiposHabitacion", "Modificar"));
    });

    options.AddPolicy("TiposHabitacionEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("TiposHabitacion", "Eliminar"));
    });
    options.AddPolicy("CatalogoConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Catalogo", "Consultar"));
    });

    options.AddPolicy("CatalogoCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Catalogo", "Crear"));
    });

    options.AddPolicy("CatalogoModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Catalogo", "Modificar"));
    });

    options.AddPolicy("CatalogoEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Catalogo", "Eliminar"));
    });
    options.AddPolicy("FacturasConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Facturas", "Consultar"));
    });

    options.AddPolicy("FacturasCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Facturas", "Crear"));
    });

    options.AddPolicy("FacturasModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Facturas", "Modificar"));
    });

    options.AddPolicy("FacturasEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("Facturas", "Eliminar"));
    });

    options.AddPolicy("DetalleFacturaConsultar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleFactura", "Consultar"));
    });

    options.AddPolicy("DetalleFacturaCrear", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleFactura", "Crear"));
    });

    options.AddPolicy("DetalleFacturaModificar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleFactura", "Modificar"));
    });

    options.AddPolicy("DetalleFacturaEliminar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("DetalleFactura", "Eliminar"));
    });

    });
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();




