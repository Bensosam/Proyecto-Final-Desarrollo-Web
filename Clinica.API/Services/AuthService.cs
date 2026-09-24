using Clinica.API.Data;
using Clinica.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Clinica.API.Services
{
    public class AuthService
    {
        private readonly ClinicaDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            ClinicaDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string?> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.UsuarioLogin == request.UsuarioLogin &&
                    u.Estado);

            if (usuario == null)
                return null;

            bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(
                request.Password,
                usuario.PasswordHash);

            if (!passwordCorrecta)
                return null;

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.UsuarioLogin),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol?.Nombre ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}