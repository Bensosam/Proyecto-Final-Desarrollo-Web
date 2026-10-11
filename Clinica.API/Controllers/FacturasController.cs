using Clinica.API.Data;
using Clinica.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturasController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public FacturasController(ClinicaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult> GetFacturas()
        {
            var facturas = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Usuario)
                .OrderByDescending(f => f.FechaFactura)
                .Select(f => new
                {
                    f.IdFactura,
                    f.NumeroFactura,
                    f.IdPaciente,
                    Paciente = f.Paciente != null
                        ? f.Paciente.Nombres + " " + f.Paciente.Apellidos
                        : null,
                    f.IdUsuario,
                    Usuario = f.Usuario != null
                        ? f.Usuario.UsuarioLogin
                        : null,
                    f.FechaFactura,
                    f.Total,
                    f.Estado
                })
                .ToListAsync();

            return Ok(facturas);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FacturasConsultar")]
        public async Task<IActionResult> GetFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Usuario)
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Catalogo)
                .Where(f => f.IdFactura == id)
                .Select(f => new
                {
                    f.IdFactura,
                    f.NumeroFactura,
                    f.IdPaciente,
                    Paciente = f.Paciente != null
                        ? f.Paciente.Nombres + " " + f.Paciente.Apellidos
                        : null,
                    f.IdUsuario,
                    Usuario = f.Usuario != null
                        ? f.Usuario.UsuarioLogin
                        : null,
                    f.FechaFactura,
                    f.Total,
                    f.Estado,
                    Detalles = f.Detalles.Select(d => new
                    {
                        d.IdDetalleFactura,
                        d.IdCatalogo,
                        Codigo = d.Catalogo != null
                            ? d.Catalogo.Codigo
                            : null,
                        d.Descripcion,
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Total
                    })
                })
                .FirstOrDefaultAsync();

            if (factura == null)
            {
                return NotFound();
            }

            return Ok(factura);
        }

        [HttpPost]
        [Authorize(Policy = "FacturasCrear")]
        public async Task<IActionResult> CrearFactura(Factura factura)
        {
            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == factura.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.IdUsuario == factura.IdUsuario &&
                    u.Estado);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe o está inactivo."
                });
            }

            if (factura.FechaFactura == default)
            {
                factura.FechaFactura = DateTime.Now;
            }

	    factura.NumeroFactura =
    	    $"TMP-{Guid.NewGuid().ToString("N")[..20]}";

            factura.Total = 0;
            factura.Estado = "Emitida";

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            factura.NumeroFactura =
                $"FAC-{factura.IdFactura:D6}";

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFactura),
                new { id = factura.IdFactura },
                new
                {
                    factura.IdFactura,
                    factura.NumeroFactura,
                    factura.IdPaciente,
                    factura.IdUsuario,
                    factura.FechaFactura,
                    factura.Total,
                    factura.Estado
                });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FacturasModificar")]
        public async Task<IActionResult> ModificarFactura(
            int id,
            Factura factura)
        {
            if (id != factura.IdFactura)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID de la factura."
                });
            }

            var existente = await _context.Facturas
                .FindAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.IdPaciente == factura.IdPaciente);

            if (!pacienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El paciente indicado no existe."
                });
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.IdUsuario == factura.IdUsuario &&
                    u.Estado);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe o está inactivo."
                });
            }

            var estadosPermitidos = new[]
            {
                "Emitida",
                "Pagada",
                "Anulada"
            };

            if (!estadosPermitidos.Contains(factura.Estado))
            {
                return BadRequest(new
                {
                    mensaje = "El estado debe ser Emitida, Pagada o Anulada."
                });
            }

            existente.IdPaciente = factura.IdPaciente;
            existente.IdUsuario = factura.IdUsuario;
            existente.FechaFactura = factura.FechaFactura;
            existente.Estado = factura.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                existente.IdFactura,
                existente.NumeroFactura,
                existente.IdPaciente,
                existente.IdUsuario,
                existente.FechaFactura,
                existente.Total,
                existente.Estado
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "FacturasEliminar")]
        public async Task<IActionResult> EliminarFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            if (factura.Detalles.Any())
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar una factura que ya tiene detalles."
                });
            }

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}