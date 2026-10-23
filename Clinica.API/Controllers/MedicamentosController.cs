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

        public MedicamentosController(
            ClinicaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CATÁLOGO MÓVIL / PWA
        // =========================================================

        [HttpGet("catalogo-movil")]
        [Authorize(Policy = "MedicamentosConsultar")]
        public async Task<IActionResult> GetCatalogoMovil()
        {
            var hoy =
                ObtenerFechaGuatemala().Date;

            var medicamentos =
                await _context.Medicamentos
                    .AsNoTracking()
                    .Where(m => m.Estado)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new
                    {
                        m.IdMedicamento,
                        m.Nombre,
                        m.Descripcion,

                        Precio =
                            m.PrecioVenta,

                        m.Imagen,
                        m.IdMarca,

                        Marca =
                            m.Marca != null
                                ? m.Marca.Nombre
                                : string.Empty,

                        m.IdCategoria,

                        Categoria =
                            m.Categoria != null
                                ? m.Categoria.Nombre
                                : string.Empty,

                        Existencia =
                            _context.LotesMedicamento
                                .Where(l =>
                                    l.IdMedicamento ==
                                        m.IdMedicamento &&
                                    l.FechaIngreso.Date <= hoy &&
                                    l.FechaVencimiento.Date >= hoy &&
                                    l.CantidadDisponible > 0)
                                .Sum(l =>
                                    (int?)l.CantidadDisponible)
                            ?? 0
                    })
                    .ToListAsync();

            return Ok(medicamentos);
        }

        // =========================================================
        // TODOS
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "MedicamentosConsultar")]
        public async Task<IActionResult> GetMedicamentos()
        {
            var medicamentos =
                await _context.Medicamentos
                    .AsNoTracking()
                    .OrderBy(m => m.Nombre)
                    .Select(m => new
                    {
                        m.IdMedicamento,
                        m.IdCategoria,

                        Categoria =
                            m.Categoria != null
                                ? m.Categoria.Nombre
                                : null,

                        m.IdMarca,

                        Marca =
                            m.Marca != null
                                ? m.Marca.Nombre
                                : null,

                        NombreMarca =
                            m.Nombre +
                            " — " +
                            (m.Marca != null
                                ? m.Marca.Nombre
                                : "Sin marca"),

                        CodigoCatalogo =
                            _context.Catalogos
                                .Where(c =>
                                    c.IdMedicamento ==
                                        m.IdMedicamento &&
                                    c.Tipo == "Producto")
                                .OrderBy(c =>
                                    c.IdCatalogo)
                                .Select(c =>
                                    c.Codigo)
                                .FirstOrDefault(),

                        m.Nombre,
                        m.Descripcion,
                        m.PrecioVenta,
                        m.Imagen,
                        m.Estado
                    })
                    .ToListAsync();

            return Ok(medicamentos);
        }

        // =========================================================
        // ACTIVOS PARA SELECTORES
        // =========================================================

        [HttpGet("activos")]
        [Authorize(Policy = "MedicamentosConsultar")]
        public async Task<IActionResult> GetMedicamentosActivos()
        {
            var medicamentos =
                await _context.Medicamentos
                    .AsNoTracking()
                    .Where(m => m.Estado)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new
                    {
                        m.IdMedicamento,
                        m.IdCategoria,

                        Categoria =
                            m.Categoria != null
                                ? m.Categoria.Nombre
                                : null,

                        m.IdMarca,

                        Marca =
                            m.Marca != null
                                ? m.Marca.Nombre
                                : null,

                        NombreMarca =
                            m.Nombre +
                            " — " +
                            (m.Marca != null
                                ? m.Marca.Nombre
                                : "Sin marca"),

                        CodigoCatalogo =
                            _context.Catalogos
                                .Where(c =>
                                    c.IdMedicamento ==
                                        m.IdMedicamento &&
                                    c.Tipo == "Producto")
                                .OrderBy(c =>
                                    c.IdCatalogo)
                                .Select(c =>
                                    c.Codigo)
                                .FirstOrDefault(),

                        m.Nombre,
                        m.Descripcion,
                        m.PrecioVenta,
                        m.Imagen,
                        m.Estado
                    })
                    .ToListAsync();

            return Ok(medicamentos);
        }

        // =========================================================
        // POR ID
        // =========================================================

        [HttpGet("{id:int}")]
        [Authorize(Policy = "MedicamentosConsultar")]
        public async Task<IActionResult> GetMedicamento(
            int id)
        {
            var medicamento =
                await _context.Medicamentos
                    .AsNoTracking()
                    .Where(m =>
                        m.IdMedicamento == id)
                    .Select(m => new
                    {
                        m.IdMedicamento,
                        m.IdCategoria,

                        Categoria =
                            m.Categoria != null
                                ? m.Categoria.Nombre
                                : null,

                        m.IdMarca,

                        Marca =
                            m.Marca != null
                                ? m.Marca.Nombre
                                : null,

                        NombreMarca =
                            m.Nombre +
                            " — " +
                            (m.Marca != null
                                ? m.Marca.Nombre
                                : "Sin marca"),

                        CodigoCatalogo =
                            _context.Catalogos
                                .Where(c =>
                                    c.IdMedicamento ==
                                        m.IdMedicamento &&
                                    c.Tipo == "Producto")
                                .OrderBy(c =>
                                    c.IdCatalogo)
                                .Select(c =>
                                    c.Codigo)
                                .FirstOrDefault(),

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

        // =========================================================
        // CREAR MEDICAMENTO + PRODUCTO DE CATÁLOGO
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "MedicamentosCrear")]
        public async Task<IActionResult> CrearMedicamento(
            Medicamento medicamento)
        {
            var error =
                await ValidarMedicamentoAsync(
                    medicamento);

            if (error != null)
            {
                return BadRequest(new
                {
                    mensaje = error
                });
            }

            var codigo =
                medicamento.CodigoCatalogo.Trim();

            var codigoExiste =
                await _context.Catalogos
                    .AnyAsync(c =>
                        c.Codigo == codigo);

            if (codigoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe un elemento del catálogo con ese código."
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                medicamento.Nombre =
                    medicamento.Nombre.Trim();

                medicamento.Descripcion =
                    medicamento.Descripcion?.Trim()
                    ?? string.Empty;

                _context.Medicamentos.Add(
                    medicamento);

                await _context.SaveChangesAsync();

                var itemCatalogo =
                    new Catalogo
                    {
                        Codigo =
                            codigo,

                        Nombre =
                            medicamento.Nombre,

                        Descripcion =
                            medicamento.Descripcion,

                        Tipo =
                            "Producto",

                        Precio =
                            medicamento.PrecioVenta,

                        Estado =
                            medicamento.Estado,

                        IdMedicamento =
                            medicamento.IdMedicamento,

                        IdTipoHabitacion =
                            null
                    };

                _context.Catalogos.Add(
                    itemCatalogo);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetMedicamento),
                    new
                    {
                        id =
                            medicamento.IdMedicamento
                    },
                    new
                    {
                        medicamento.IdMedicamento,
                        medicamento.IdCategoria,
                        medicamento.IdMarca,
                        medicamento.Nombre,
                        medicamento.Descripcion,
                        medicamento.PrecioVenta,
                        medicamento.Imagen,
                        medicamento.Estado,

                        CodigoCatalogo =
                            itemCatalogo.Codigo,

                        IdCatalogo =
                            itemCatalogo.IdCatalogo
                    });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // MODIFICAR MEDICAMENTO + SINCRONIZAR CATÁLOGO
        // =========================================================

        [HttpPut("{id:int}")]
        [Authorize(Policy = "MedicamentosModificar")]
        public async Task<IActionResult> ModificarMedicamento(
            int id,
            Medicamento medicamento)
        {
            if (id !=
                medicamento.IdMedicamento)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El ID de la URL no coincide con el ID del medicamento."
                });
            }

            var existente =
                await _context.Medicamentos
                    .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var error =
                await ValidarMedicamentoAsync(
                    medicamento);

            if (error != null)
            {
                return BadRequest(new
                {
                    mensaje = error
                });
            }

            var codigo =
                medicamento.CodigoCatalogo.Trim();

            var itemCatalogo =
                await _context.Catalogos
                    .Where(c =>
                        c.IdMedicamento == id &&
                        c.Tipo == "Producto")
                    .OrderBy(c =>
                        c.IdCatalogo)
                    .FirstOrDefaultAsync();

            var codigoExiste =
                await _context.Catalogos
                    .AnyAsync(c =>
                        c.Codigo == codigo &&
                        (
                            itemCatalogo == null ||
                            c.IdCatalogo !=
                                itemCatalogo.IdCatalogo
                        ));

            if (codigoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otro elemento del catálogo con ese código."
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                existente.IdCategoria =
                    medicamento.IdCategoria;

                existente.IdMarca =
                    medicamento.IdMarca;

                existente.Nombre =
                    medicamento.Nombre.Trim();

                existente.Descripcion =
                    medicamento.Descripcion?.Trim()
                    ?? string.Empty;

                existente.PrecioVenta =
                    medicamento.PrecioVenta;

                existente.Imagen =
                    medicamento.Imagen;

                existente.Estado =
                    medicamento.Estado;

                if (itemCatalogo == null)
                {
                    itemCatalogo =
                        new Catalogo
                        {
                            Codigo =
                                codigo,

                            Tipo =
                                "Producto",

                            IdMedicamento =
                                existente.IdMedicamento,

                            IdTipoHabitacion =
                                null
                        };

                    _context.Catalogos.Add(
                        itemCatalogo);
                }

                itemCatalogo.Codigo =
                    codigo;

                itemCatalogo.Nombre =
                    existente.Nombre;

                itemCatalogo.Descripcion =
                    existente.Descripcion;

                itemCatalogo.Tipo =
                    "Producto";

                itemCatalogo.Precio =
                    existente.PrecioVenta;

                itemCatalogo.Estado =
                    existente.Estado;

                itemCatalogo.IdMedicamento =
                    existente.IdMedicamento;

                itemCatalogo.IdTipoHabitacion =
                    null;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    existente.IdMedicamento,
                    existente.IdCategoria,
                    existente.IdMarca,
                    existente.Nombre,
                    existente.Descripcion,
                    existente.PrecioVenta,
                    existente.Imagen,
                    existente.Estado,

                    CodigoCatalogo =
                        itemCatalogo.Codigo,

                    IdCatalogo =
                        itemCatalogo.IdCatalogo
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // ELIMINAR
        // Si el producto ya fue facturado, no se elimina.
        // =========================================================

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "MedicamentosEliminar")]
        public async Task<IActionResult> EliminarMedicamento(
            int id)
        {
            var medicamento =
                await _context.Medicamentos
                    .FindAsync(id);

            if (medicamento == null)
            {
                return NotFound();
            }

            var catalogos =
                await _context.Catalogos
                    .Where(c =>
                        c.IdMedicamento == id)
                    .ToListAsync();

            var idsCatalogo =
                catalogos
                    .Select(c =>
                        c.IdCatalogo)
                    .ToList();

            if (idsCatalogo.Count > 0)
            {
                var utilizado =
                    await _context.DetallesFactura
                        .AnyAsync(d =>
                            idsCatalogo.Contains(
                                d.IdCatalogo));

                if (utilizado)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "No se puede eliminar el medicamento porque su producto de catálogo ya fue utilizado en una factura. Desactívelo en su lugar."
                    });
                }
            }

            var tieneLotes =
                await _context.LotesMedicamento
                    .AnyAsync(l =>
                        l.IdMedicamento == id);

            if (tieneLotes)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar el medicamento porque tiene lotes asociados. Desactívelo en su lugar."
                });
            }

            var tieneRecetas =
                await _context.DetallesReceta
                    .AnyAsync(d =>
                        d.IdMedicamento == id);

            if (tieneRecetas)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar el medicamento porque aparece en recetas. Desactívelo en su lugar."
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                if (catalogos.Count > 0)
                {
                    _context.Catalogos.RemoveRange(
                        catalogos);
                }

                _context.Medicamentos.Remove(
                    medicamento);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // VALIDACIONES
        // =========================================================

        private async Task<string?>
            ValidarMedicamentoAsync(
                Medicamento medicamento)
        {
            if (string.IsNullOrWhiteSpace(
                    medicamento.Nombre))
            {
                return
                    "El nombre del medicamento es obligatorio.";
            }

            if (string.IsNullOrWhiteSpace(
                    medicamento.CodigoCatalogo))
            {
                return
                    "El código de facturación es obligatorio.";
            }

            if (medicamento.PrecioVenta < 0)
            {
                return
                    "El precio de venta no puede ser negativo.";
            }

            var categoriaExiste =
                await _context.Categorias
                    .AnyAsync(c =>
                        c.IdCategoria ==
                            medicamento.IdCategoria &&
                        c.Estado);

            if (!categoriaExiste)
            {
                return
                    "La categoría indicada no existe o está inactiva.";
            }

            var marcaExiste =
                await _context.Marcas
                    .AnyAsync(m =>
                        m.IdMarca ==
                            medicamento.IdMarca &&
                        m.Estado);

            if (!marcaExiste)
            {
                return
                    "La marca indicada no existe o está inactiva.";
            }

            return null;
        }

        private static DateTime ObtenerFechaGuatemala()
        {
            var idZona =
                OperatingSystem.IsWindows()
                    ? "Central America Standard Time"
                    : "America/Guatemala";

            var zona =
                TimeZoneInfo.FindSystemTimeZoneById(
                    idZona);

            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                zona);
        }
    }
}
