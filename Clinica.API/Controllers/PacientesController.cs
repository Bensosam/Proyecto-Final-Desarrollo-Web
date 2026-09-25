using Clinica.API.Authorization;
using Clinica.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public PacientesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "PacientesConsultar")]
        public async Task<IActionResult> GetPacientes()
        {
            var pacientes = await _context.Pacientes.ToListAsync();

            return Ok(pacientes);
        }
    }
}
