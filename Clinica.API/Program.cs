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




