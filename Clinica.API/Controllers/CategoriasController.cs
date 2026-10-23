using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public CategoriasController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "CategoriasConsultar")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias =
                await _context.Categorias
                    .OrderBy(c => c.Nombre)
                    .Select(c => new
                    {
                        c.IdCategoria,
                        c.Nombre,
                        c.Descripcion,
                        c.Estado
                    })
                    .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("activos")]
        [Authorize(Policy = "CategoriasConsultar")]
        public async Task<IActionResult> GetCategoriasActivas()
        {
            var categorias =
                await _context.Categorias
                    .Where(c => c.Estado)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new
                    {
                        c.IdCategoria,
                        c.Nombre,
                        c.Descripcion,
                        c.Estado
                    })
                    .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "CategoriasConsultar")]
        public async Task<IActionResult> GetCategoria(
            int id)
        {
            var categoria =
                await _context.Categorias
                    .Where(c =>
                        c.IdCategoria == id)
                    .Select(c => new
                    {
                        c.IdCategoria,
                        c.Nombre,
                        c.Descripcion,
                        c.Estado
                    })
                    .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        [HttpPost]
        [Authorize(Policy = "CategoriasCrear")]
        public async Task<IActionResult> CrearCategoria(
            Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(
                    categoria.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre de la categoría es obligatorio."
                });
            }

            categoria.Nombre =
                categoria.Nombre.Trim();

            categoria.Descripcion =
                categoria.Descripcion?.Trim()
                ?? string.Empty;

            var existe =
                await _context.Categorias
                    .AnyAsync(c =>
                        c.Nombre ==
                        categoria.Nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe una categoría con ese nombre."
                });
            }

            _context.Categorias.Add(
                categoria);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoria),
                new
                {
                    id =
                        categoria.IdCategoria
                },
                new
                {
                    categoria.IdCategoria,
                    categoria.Nombre,
                    categoria.Descripcion,
                    categoria.Estado
                });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "CategoriasModificar")]
        public async Task<IActionResult> ModificarCategoria(
            int id,
            Categoria categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID de la categoría."
                });
            }

            var existente =
                await _context.Categorias
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(
                    categoria.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre de la categoría es obligatorio."
                });
            }

            var nombre =
                categoria.Nombre.Trim();

            var nombreExiste =
                await _context.Categorias
                    .AnyAsync(c =>
                        c.Nombre == nombre &&
                        c.IdCategoria != id);

            if (nombreExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otra categoría con ese nombre."
                });
            }

            if (!categoria.Estado)
            {
                var tieneMedicamentosActivos =
                    await _context.Medicamentos
                        .AnyAsync(m =>
                            m.IdCategoria == id &&
                            m.Estado);

                if (tieneMedicamentosActivos)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "No se puede desactivar la categoría porque tiene medicamentos activos asociados."
                    });
                }
            }

            existente.Nombre =
                nombre;

            existente.Descripcion =
                categoria.Descripcion?.Trim()
                ?? string.Empty;

            existente.Estado =
                categoria.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdCategoria,
                existente.Nombre,
                existente.Descripcion,
                existente.Estado
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "CategoriasEliminar")]
        public async Task<IActionResult> EliminarCategoria(
            int id)
        {
            var categoria =
                await _context.Categorias
                    .FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            var utilizada =
                await _context.Medicamentos
                    .AnyAsync(m =>
                        m.IdCategoria == id);

            if (utilizada)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar la categoría porque está asociada a medicamentos."
                });
            }

            _context.Categorias.Remove(
                categoria);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
