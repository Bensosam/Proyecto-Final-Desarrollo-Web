using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogoController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public CatalogoController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "CatalogoConsultar")]
        public async Task<IActionResult> GetCatalogo()
        {
            var catalogo = await _context.Catalogos
                .Include(c => c.Medicamento)
                .Include(c => c.TipoHabitacion)
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Nombre)
                .Select(c => new
                {
                    c.IdCatalogo,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.Tipo,
                    c.Precio,
                    c.Estado,
                    c.IdMedicamento,
                    Medicamento = c.Medicamento != null
                        ? c.Medicamento.Nombre
                        : null,
                    c.IdTipoHabitacion,
                    TipoHabitacion = c.TipoHabitacion != null
                        ? c.TipoHabitacion.Nombre
                        : null
                })
                .ToListAsync();

            return Ok(catalogo);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "CatalogoConsultar")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.Catalogos
                .Include(c => c.Medicamento)
                .Include(c => c.TipoHabitacion)
                .Where(c => c.IdCatalogo == id)
                .Select(c => new
                {
                    c.IdCatalogo,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.Tipo,
                    c.Precio,
                    c.Estado,
                    c.IdMedicamento,
                    Medicamento = c.Medicamento != null
                        ? c.Medicamento.Nombre
                        : null,
                    c.IdTipoHabitacion,
                    TipoHabitacion = c.TipoHabitacion != null
                        ? c.TipoHabitacion.Nombre
                        : null
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        [Authorize(Policy = "CatalogoCrear")]
        public async Task<IActionResult> CrearItem(Catalogo item)
        {
            if (string.IsNullOrWhiteSpace(item.Codigo))
            {
                return BadRequest(new
                {
                    mensaje = "El código es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(item.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre es obligatorio."
                });
            }

            if (item.Tipo != "Producto" &&
                item.Tipo != "Servicio")
            {
                return BadRequest(new
                {
                    mensaje = "El tipo debe ser Producto o Servicio."
                });
            }

            if (item.Precio < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio no puede ser negativo."
                });
            }

            var codigoExiste = await _context.Catalogos
                .AnyAsync(c => c.Codigo == item.Codigo);

            if (codigoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe un elemento con ese código."
                });
            }

            if (item.Tipo == "Producto")
            {
                if (item.IdMedicamento == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "Un producto debe estar relacionado con un medicamento."
                    });
                }

                var medicamentoExiste = await _context.Medicamentos
                    .AnyAsync(m =>
                        m.IdMedicamento == item.IdMedicamento &&
                        m.Estado);

                if (!medicamentoExiste)
                {
                    return BadRequest(new
                    {
                        mensaje = "El medicamento indicado no existe o está inactivo."
                    });
                }

                item.IdTipoHabitacion = null;
            }
            else
            {
                item.IdMedicamento = null;

                if (item.IdTipoHabitacion != null)
                {
                    var tipoExiste = await _context.TiposHabitacion
                        .AnyAsync(t =>
                            t.IdTipoHabitacion == item.IdTipoHabitacion &&
                            t.Estado);

                    if (!tipoExiste)
                    {
                        return BadRequest(new
                        {
                            mensaje = "El tipo de habitación indicado no existe o está inactivo."
                        });
                    }
                }
            }

            _context.Catalogos.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetItem),
                new { id = item.IdCatalogo },
                new
                {
                    item.IdCatalogo,
                    item.Codigo,
                    item.Nombre,
                    item.Descripcion,
                    item.Tipo,
                    item.Precio,
                    item.Estado,
                    item.IdMedicamento,
                    item.IdTipoHabitacion
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CatalogoModificar")]
        public async Task<IActionResult> ModificarItem(
            int id,
            Catalogo item)
        {
            if (id != item.IdCatalogo)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del catálogo."
                });
            }

            var existente = await _context.Catalogos.FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(item.Codigo) ||
                string.IsNullOrWhiteSpace(item.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "Código y nombre son obligatorios."
                });
            }

            if (item.Tipo != "Producto" &&
                item.Tipo != "Servicio")
            {
                return BadRequest(new
                {
                    mensaje = "El tipo debe ser Producto o Servicio."
                });
            }

            if (item.Precio < 0)
            {
                return BadRequest(new
                {
                    mensaje = "El precio no puede ser negativo."
                });
            }

            var codigoExiste = await _context.Catalogos
                .AnyAsync(c =>
                    c.Codigo == item.Codigo &&
                    c.IdCatalogo != id);

            if (codigoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe otro elemento con ese código."
                });
            }

            if (item.Tipo == "Producto")
            {
                if (item.IdMedicamento == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "Un producto debe estar relacionado con un medicamento."
                    });
                }

                var medicamentoExiste = await _context.Medicamentos
                    .AnyAsync(m =>
                        m.IdMedicamento == item.IdMedicamento &&
                        m.Estado);

                if (!medicamentoExiste)
                {
                    return BadRequest(new
                    {
                        mensaje = "El medicamento indicado no existe o está inactivo."
                    });
                }

                item.IdTipoHabitacion = null;
            }
            else
            {
                item.IdMedicamento = null;

                if (item.IdTipoHabitacion != null)
                {
                    var tipoExiste = await _context.TiposHabitacion
                        .AnyAsync(t =>
                            t.IdTipoHabitacion == item.IdTipoHabitacion &&
                            t.Estado);

                    if (!tipoExiste)
                    {
                        return BadRequest(new
                        {
                            mensaje = "El tipo de habitación indicado no existe o está inactivo."
                        });
                    }
                }
            }

            existente.Codigo = item.Codigo;
            existente.Nombre = item.Nombre;
            existente.Descripcion = item.Descripcion;
            existente.Tipo = item.Tipo;
            existente.Precio = item.Precio;
            existente.Estado = item.Estado;
            existente.IdMedicamento = item.IdMedicamento;
            existente.IdTipoHabitacion = item.IdTipoHabitacion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdCatalogo,
                existente.Codigo,
                existente.Nombre,
                existente.Descripcion,
                existente.Tipo,
                existente.Precio,
                existente.Estado,
                existente.IdMedicamento,
                existente.IdTipoHabitacion
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CatalogoEliminar")]
        public async Task<IActionResult> EliminarItem(int id)
        {
            var item = await _context.Catalogos.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            var utilizado = await _context.DetallesFactura
                .AnyAsync(d => d.IdCatalogo == id);

            if (utilizado)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar un elemento que ya aparece en una factura."
                });
            }

            _context.Catalogos.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}