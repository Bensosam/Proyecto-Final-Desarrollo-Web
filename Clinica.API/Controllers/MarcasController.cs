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

        public MarcasController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "MarcasConsultar")]
        public async Task<IActionResult> GetMarcas()
        {
            var marcas =
                await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new
                    {
                        m.IdMarca,
                        m.Nombre,
                        m.Descripcion,
                        m.Estado
                    })
                    .ToListAsync();

            return Ok(marcas);
        }

        [HttpGet("activos")]
        [Authorize(Policy = "MarcasConsultar")]
        public async Task<IActionResult> GetMarcasActivas()
        {
            var marcas =
                await _context.Marcas
                    .Where(m => m.Estado)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new
                    {
                        m.IdMarca,
                        m.Nombre,
                        m.Descripcion,
                        m.Estado
                    })
                    .ToListAsync();

            return Ok(marcas);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "MarcasConsultar")]
        public async Task<IActionResult> GetMarca(
            int id)
        {
            var marca =
                await _context.Marcas
                    .Where(m =>
                        m.IdMarca == id)
                    .Select(m => new
                    {
                        m.IdMarca,
                        m.Nombre,
                        m.Descripcion,
                        m.Estado
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
        public async Task<IActionResult> CrearMarca(
            Marca marca)
        {
            if (string.IsNullOrWhiteSpace(
                    marca.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre de la marca es obligatorio."
                });
            }

            marca.Nombre =
                marca.Nombre.Trim();

            marca.Descripcion =
                marca.Descripcion?.Trim()
                ?? string.Empty;

            var existe =
                await _context.Marcas
                    .AnyAsync(m =>
                        m.Nombre ==
                        marca.Nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe una marca con ese nombre."
                });
            }

            _context.Marcas.Add(
                marca);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMarca),
                new
                {
                    id =
                        marca.IdMarca
                },
                new
                {
                    marca.IdMarca,
                    marca.Nombre,
                    marca.Descripcion,
                    marca.Estado
                });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "MarcasModificar")]
        public async Task<IActionResult> ModificarMarca(
            int id,
            Marca marca)
        {
            if (id != marca.IdMarca)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID de la marca."
                });
            }

            var existente =
                await _context.Marcas
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(
                    marca.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre de la marca es obligatorio."
                });
            }

            var nombre =
                marca.Nombre.Trim();

            var nombreExiste =
                await _context.Marcas
                    .AnyAsync(m =>
                        m.Nombre == nombre &&
                        m.IdMarca != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otra marca con ese nombre."
                });
            }

            if (!marca.Estado)
            {
                var tieneMedicamentosActivos =
                    await _context.Medicamentos
                        .AnyAsync(m =>
                            m.IdMarca == id &&
                            m.Estado);

                if (tieneMedicamentosActivos)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "No se puede desactivar la marca porque tiene medicamentos activos asociados."
                    });
                }
            }

            existente.Nombre =
                nombre;

            existente.Descripcion =
                marca.Descripcion?.Trim()
                ?? string.Empty;

            existente.Estado =
                marca.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdMarca,
                existente.Nombre,
                existente.Descripcion,
                existente.Estado
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "MarcasEliminar")]
        public async Task<IActionResult> EliminarMarca(
            int id)
        {
            var marca =
                await _context.Marcas
                    .FindAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            var utilizada =
                await _context.Medicamentos
                    .AnyAsync(m =>
                        m.IdMarca == id);

            if (utilizada)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar la marca porque está asociada a medicamentos."
                });
            }

            _context.Marcas.Remove(
                marca);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
