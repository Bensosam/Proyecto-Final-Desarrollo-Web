using Clinica.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public TurnosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTurnos()
        {
            var turnos = await _context.Turnos
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTurno)
                .Select(t => new
                {
                    t.IdTurno,
                    t.Nombre,
                    t.HoraInicio,
                    t.HoraFin,
                    t.Estado
                })
                .ToListAsync();

            return Ok(turnos);
        }
    }
}