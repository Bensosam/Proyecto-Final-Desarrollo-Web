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

        public CatalogoController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CONSULTAR CATALOGO
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "CatalogoConsultar")]
        public async Task<IActionResult> GetCatalogo()
        {
            var catalogo =
                await _context.Catalogos
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

                        Medicamento =
                            c.Medicamento != null
                                ? c.Medicamento.Nombre
                                : null,

                        c.IdTipoHabitacion,

                        TipoHabitacion =
                            c.TipoHabitacion != null
                                ? c.TipoHabitacion.Nombre
                                : null,

                        AdministradoPorMedicamentos =
                            c.Tipo == "Producto" &&
                            c.IdMedicamento != null
                    })
                    .ToListAsync();

            return Ok(catalogo);
        }

        // =========================================================
        // CONSULTAR ELEMENTO
        // =========================================================

        [HttpGet("{id:int}")]
        [Authorize(Policy = "CatalogoConsultar")]
        public async Task<IActionResult> GetItem(
            int id)
        {
            var item =
                await _context.Catalogos
                    .Include(c => c.Medicamento)
                    .Include(c => c.TipoHabitacion)
                    .Where(c =>
                        c.IdCatalogo == id)
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

                        Medicamento =
                            c.Medicamento != null
                                ? c.Medicamento.Nombre
                                : null,

                        c.IdTipoHabitacion,

                        TipoHabitacion =
                            c.TipoHabitacion != null
                                ? c.TipoHabitacion.Nombre
                                : null,

                        AdministradoPorMedicamentos =
                            c.Tipo == "Producto" &&
                            c.IdMedicamento != null
                    })
                    .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // =========================================================
        // CREAR
        //
        // Los productos de medicamentos NO se crean aquí.
        // MedicamentosController es la fuente de verdad.
        // Aquí se crean servicios.
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "CatalogoCrear")]
        public async Task<IActionResult> CrearItem(
            Catalogo item)
        {
            if (string.IsNullOrWhiteSpace(
                    item.Codigo))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El código es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    item.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre es obligatorio."
                });
            }

            item.Codigo =
                item.Codigo.Trim();

            item.Nombre =
                item.Nombre.Trim();

            item.Descripcion =
                item.Descripcion?.Trim()
                ?? string.Empty;

            if (item.Tipo != "Producto" &&
                item.Tipo != "Servicio")
            {
                return BadRequest(new
                {
                    mensaje =
                        "El tipo debe ser Producto o Servicio."
                });
            }

            // -----------------------------------------------------
            // PRODUCTOS: administrados exclusivamente desde
            // Medicamentos.
            // -----------------------------------------------------

            if (item.Tipo == "Producto")
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los productos de medicamentos se crean automáticamente desde el módulo Medicamentos. No deben crearse manualmente desde Catálogo."
                });
            }

            if (item.Precio < 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El precio no puede ser negativo."
                });
            }

            var codigoExiste =
                await _context.Catalogos
                    .AnyAsync(c =>
                        c.Codigo == item.Codigo);

            if (codigoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe un elemento con ese código."
                });
            }

            // Un servicio nunca se vincula a medicamento.
            item.IdMedicamento = null;
            item.Tipo = "Servicio";

            if (item.IdTipoHabitacion != null)
            {
                var tipoExiste =
                    await _context.TiposHabitacion
                        .AnyAsync(t =>
                            t.IdTipoHabitacion ==
                                item.IdTipoHabitacion &&
                            t.Estado);

                if (!tipoExiste)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El tipo de habitación indicado no existe o está inactivo."
                    });
                }
            }

            _context.Catalogos.Add(item);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetItem),
                new
                {
                    id =
                        item.IdCatalogo
                },
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

        // =========================================================
        // MODIFICAR
        // =========================================================

        [HttpPut("{id:int}")]
        [Authorize(Policy = "CatalogoModificar")]
        public async Task<IActionResult> ModificarItem(
            int id,
            Catalogo item)
        {
            if (id != item.IdCatalogo)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del catálogo."
                });
            }

            var existente =
                await _context.Catalogos
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Si está ligado a un medicamento, Medicamentos es
            // la única fuente autorizada para cambiarlo.
            // -----------------------------------------------------

            if (existente.Tipo == "Producto" &&
                existente.IdMedicamento.HasValue)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Este producto es administrado automáticamente desde Medicamentos. Modifique el medicamento para actualizar código, nombre, descripción, precio o estado."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    item.Codigo) ||
                string.IsNullOrWhiteSpace(
                    item.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Código y nombre son obligatorios."
                });
            }

            item.Codigo =
                item.Codigo.Trim();

            item.Nombre =
                item.Nombre.Trim();

            item.Descripcion =
                item.Descripcion?.Trim()
                ?? string.Empty;

            if (item.Tipo != "Producto" &&
                item.Tipo != "Servicio")
            {
                return BadRequest(new
                {
                    mensaje =
                        "El tipo debe ser Producto o Servicio."
                });
            }

            // Tampoco permitimos convertir manualmente un
            // servicio en producto.
            if (item.Tipo == "Producto")
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los productos de medicamentos se administran desde el módulo Medicamentos."
                });
            }

            if (item.Precio < 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El precio no puede ser negativo."
                });
            }

            var codigoExiste =
                await _context.Catalogos
                    .AnyAsync(c =>
                        c.Codigo ==
                            item.Codigo &&
                        c.IdCatalogo != id);

            if (codigoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otro elemento con ese código."
                });
            }

            item.IdMedicamento = null;
            item.Tipo = "Servicio";

            if (item.IdTipoHabitacion != null)
            {
                var tipoExiste =
                    await _context.TiposHabitacion
                        .AnyAsync(t =>
                            t.IdTipoHabitacion ==
                                item.IdTipoHabitacion &&
                            t.Estado);

                if (!tipoExiste)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El tipo de habitación indicado no existe o está inactivo."
                    });
                }
            }

            existente.Codigo =
                item.Codigo;

            existente.Nombre =
                item.Nombre;

            existente.Descripcion =
                item.Descripcion;

            existente.Tipo =
                "Servicio";

            existente.Precio =
                item.Precio;

            existente.Estado =
                item.Estado;

            existente.IdMedicamento =
                null;

            existente.IdTipoHabitacion =
                item.IdTipoHabitacion;

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

        // =========================================================
        // ELIMINAR
        // =========================================================

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "CatalogoEliminar")]
        public async Task<IActionResult> EliminarItem(
            int id)
        {
            var item =
                await _context.Catalogos
                    .FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            // Los productos ligados a medicamentos deben
            // administrarse desde Medicamentos.
            if (item.Tipo == "Producto" &&
                item.IdMedicamento.HasValue)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No puede eliminar este producto desde Catálogo porque está administrado por Medicamentos. Administre el medicamento correspondiente."
                });
            }

            var utilizado =
                await _context.DetallesFactura
                    .AnyAsync(d =>
                        d.IdCatalogo == id);

            if (utilizado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar un elemento que ya aparece en una factura."
                });
            }

            _context.Catalogos.Remove(
                item);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
