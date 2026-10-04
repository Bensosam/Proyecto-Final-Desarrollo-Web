using Clinica.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspecialidadesController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public EspecialidadesController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
[Authorize]
public async Task<IActionResult> GetEspecialidades()
{
    var especialidades = await _context.Especialidades
        .OrderBy(e => e.IdEspecialidad)
        .Select(e => new
        {
            e.IdEspecialidad,
            e.Nombre
        })
        .ToListAsync();

    return Ok(especialidades);

        }
    }
}