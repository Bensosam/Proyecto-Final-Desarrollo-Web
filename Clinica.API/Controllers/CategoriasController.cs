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

        public CategoriasController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "CategoriasConsultar")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.IdCategoria,
                    c.Nombre,
                    c.Descripcion
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "CategoriasConsultar")]
        public async Task<IActionResult> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.IdCategoria == id)
                .Select(c => new
                {
                    c.IdCategoria,
                    c.Nombre,
                    c.Descripcion
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
        public async Task<IActionResult> CrearCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoria.IdCategoria },
                new
                {
                    categoria.IdCategoria,
                    categoria.Nombre,
                    categoria.Descripcion
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CategoriasModificar")]
        public async Task<IActionResult> ModificarCategoria(
            int id,
            Categoria categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la categoría."
                });
            }

            var categoriaExistente = await _context.Categorias.FindAsync(id);

            if (categoriaExistente == null)
            {
                return NotFound();
            }

            categoriaExistente.Nombre = categoria.Nombre;
            categoriaExistente.Descripcion = categoria.Descripcion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                categoriaExistente.IdCategoria,
                categoriaExistente.Nombre,
                categoriaExistente.Descripcion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CategoriasEliminar")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
