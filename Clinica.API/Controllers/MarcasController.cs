using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarcasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public MarcasController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "MarcasConsultar")]
        public async Task<IActionResult> GetMarcas()
        {
            var marcas = await _context.Marcas
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.IdMarca,
                    m.Nombre
                })
                .ToListAsync();

            return Ok(marcas);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "MarcasConsultar")]
        public async Task<IActionResult> GetMarca(int id)
        {
            var marca = await _context.Marcas
                .Where(m => m.IdMarca == id)
                .Select(m => new
                {
                    m.IdMarca,
                    m.Nombre
                })
                .FirstOrDefaultAsync();

            if (marca == null)
            {
                return NotFound();
            }

            return Ok(marca);
        }

        [HttpPost]
        [Authorize(Policy = "MarcasCrear")]
        public async Task<IActionResult> CrearMarca(Marca marca)
        {
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMarca),
                new { id = marca.IdMarca },
                new
                {
                    marca.IdMarca,
                    marca.Nombre
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "MarcasModificar")]
        public async Task<IActionResult> ModificarMarca(
            int id,
            Marca marca)
        {
            if (id != marca.IdMarca)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la marca."
                });
            }

            var marcaExistente = await _context.Marcas.FindAsync(id);

            if (marcaExistente == null)
            {
                return NotFound();
            }

            marcaExistente.Nombre = marca.Nombre;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                marcaExistente.IdMarca,
                marcaExistente.Nombre
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "MarcasEliminar")]
        public async Task<IActionResult> EliminarMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
