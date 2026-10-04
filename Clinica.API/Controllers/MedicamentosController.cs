using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicamentosController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public MedicamentosController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "MedicamentosConsultar")]
        public async Task<IActionResult> GetMedicamentos()
        {
            var medicamentos = await _context.Medicamentos
                .Include(m => m.Categoria)
                .Include(m => m.Marca)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.IdMedicamento,
                    m.IdCategoria,
                    Categoria = m.Categoria != null
                        ? m.Categoria.Nombre
                        : null,
                    m.IdMarca,
                    Marca = m.Marca != null
                        ? m.Marca.Nombre
                        : null,
                    m.Nombre,
                    m.Descripcion,
                    m.PrecioVenta,
                    m.Imagen,
                    m.Estado
                })
                .ToListAsync();

            return Ok(medicamentos);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "MedicamentosConsultar")]
        public async Task<IActionResult> GetMedicamento(int id)
        {
            var medicamento = await _context.Medicamentos
                .Include(m => m.Categoria)
                .Include(m => m.Marca)
                .Where(m => m.IdMedicamento == id)
                .Select(m => new
                {
                    m.IdMedicamento,
                    m.IdCategoria,
                    Categoria = m.Categoria != null
                        ? m.Categoria.Nombre
                        : null,
                    m.IdMarca,
                    Marca = m.Marca != null
                        ? m.Marca.Nombre
                        : null,
                    m.Nombre,
                    m.Descripcion,
                    m.PrecioVenta,
                    m.Imagen,
                    m.Estado
                })
                .FirstOrDefaultAsync();

            if (medicamento == null)
            {
                return NotFound();
            }

            return Ok(medicamento);
        }

        [HttpPost]
        [Authorize(Policy = "MedicamentosCrear")]
        public async Task<IActionResult> CrearMedicamento(Medicamento medicamento)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == medicamento.IdCategoria);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La categoría indicada no existe."
                });
            }

            var marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == medicamento.IdMarca);

            if (!marcaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La marca indicada no existe."
                });
            }

            _context.Medicamentos.Add(medicamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMedicamento),
                new { id = medicamento.IdMedicamento },
                new
                {
                    medicamento.IdMedicamento,
                    medicamento.IdCategoria,
                    medicamento.IdMarca,
                    medicamento.Nombre,
                    medicamento.Descripcion,
                    medicamento.PrecioVenta,
                    medicamento.Imagen,
                    medicamento.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "MedicamentosModificar")]
        public async Task<IActionResult> ModificarMedicamento(
            int id,
            Medicamento medicamento)
        {
            if (id != medicamento.IdMedicamento)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del medicamento."
                });
            }

            var medicamentoExistente = await _context.Medicamentos
                .FindAsync(id);

            if (medicamentoExistente == null)
            {
                return NotFound();
            }

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == medicamento.IdCategoria);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La categoría indicada no existe."
                });
            }

            var marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == medicamento.IdMarca);

            if (!marcaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La marca indicada no existe."
                });
            }

            medicamentoExistente.IdCategoria = medicamento.IdCategoria;
            medicamentoExistente.IdMarca = medicamento.IdMarca;
            medicamentoExistente.Nombre = medicamento.Nombre;
            medicamentoExistente.Descripcion = medicamento.Descripcion;
            medicamentoExistente.PrecioVenta = medicamento.PrecioVenta;
            medicamentoExistente.Imagen = medicamento.Imagen;
            medicamentoExistente.Estado = medicamento.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                medicamentoExistente.IdMedicamento,
                medicamentoExistente.IdCategoria,
                medicamentoExistente.IdMarca,
                medicamentoExistente.Nombre,
                medicamentoExistente.Descripcion,
                medicamentoExistente.PrecioVenta,
                medicamentoExistente.Imagen,
                medicamentoExistente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "MedicamentosEliminar")]
        public async Task<IActionResult> EliminarMedicamento(int id)
        {
            var medicamento = await _context.Medicamentos
                .FindAsync(id);

            if (medicamento == null)
            {
                return NotFound();
            }

            _context.Medicamentos.Remove(medicamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
