using Clinica.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SucursalesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public SucursalesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSucursales()
        {
            var sucursales = await _context.Sucursales
                .OrderBy(s => s.IdSucursal)
                .Select(s => new
                {
                    s.IdSucursal,
                    s.Nombre,
                    s.Direccion,
                    s.Telefono,
                    s.Estado
                })
                .ToListAsync();

            return Ok(sucursales);
        }
    }
}