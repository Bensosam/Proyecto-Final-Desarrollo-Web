using Clinica.Web.Models;

namespace Clinica.Web.Services;

public sealed class ModuleRegistry
{
    private readonly Dictionary<string, ModuleDefinition> _modules;

    public ModuleRegistry()
    {
        _modules = Build().ToDictionary(m => m.Slug, StringComparer.OrdinalIgnoreCase);
    }

    public ModuleDefinition? Get(string slug)
        => _modules.TryGetValue(slug, out var module) ? module : null;

    public IReadOnlyCollection<ModuleDefinition> All => _modules.Values;

    private static List<ModuleDefinition> Build() =>
    [
        new()
        {
            Slug = "pacientes", Title = "Pacientes", Singular = "paciente",
            Subtitle = "Registro y administración de pacientes de la clínica.", Endpoint = "api/Pacientes", IdField = "idPaciente", Icon = "users",
            Columns = [C("idPaciente", "ID", ColumnFormat.Number), C("nombres", "Nombres"), C("apellidos", "Apellidos"), C("telefono", "Teléfono"), C("correo", "Correo"), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [
                F("nombres", "Nombres", true), F("apellidos", "Apellidos", true),
                F("fechaNacimiento", "Fecha de nacimiento", kind: FieldKind.Date), F("genero", "Género"),
                F("direccion", "Dirección", kind: FieldKind.TextArea), F("telefono", "Teléfono"), F("correo", "Correo electrónico"),
                F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")
            ]
        },
        new()
        {
            Slug = "empleados", Title = "Empleados", Singular = "empleado", Subtitle = "Personal médico y administrativo.", Endpoint = "api/Empleados", IdField = "idEmpleado", Icon = "badge",
            Columns = [C("idEmpleado", "ID"), C("nombres", "Nombres"), C("apellidos", "Apellidos"), C("sucursal", "Sucursal"), C("especialidad", "Especialidad"), C("turno", "Turno"), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [
                S("idSucursal", "Sucursal", "api/Sucursales", "idSucursal", "nombre", true),
                S("idEspecialidad", "Especialidad", "api/Especialidades", "idEspecialidad", "nombre"),
                S("idTurno", "Turno", "api/Turnos", "idTurno", "nombre", true),
                F("nombres", "Nombres", true), F("apellidos", "Apellidos", true), F("puesto", "Puesto / cargo"),
                F("telefono", "Teléfono"), F("correo", "Correo"), F("direccion", "Dirección", kind: FieldKind.TextArea),
                F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")
            ]
        },
        new()
        {
            Slug = "consultas", Title = "Consultas", Singular = "consulta", Subtitle = "Consultas médicas y seguimiento clínico.", Endpoint = "api/Consultas", IdField = "idConsulta", Icon = "stethoscope",
            Columns = [C("idConsulta", "ID"), C("paciente", "Paciente"), C("empleado", "Profesional"), C("fechaConsulta", "Fecha", ColumnFormat.DateTime), C("motivo", "Motivo")],
            Fields = [
                S("idPaciente", "Paciente", "api/Pacientes", "idPaciente", "nombres", true, "apellidos"),
                S("idEmpleado", "Empleado", "api/Empleados", "idEmpleado", "nombres", true, "apellidos"),
                F("fechaConsulta", "Fecha y hora", true, FieldKind.DateTime, defaultValue: "now"),
                F("motivo", "Motivo", true, FieldKind.TextArea), F("observaciones", "Observaciones", kind: FieldKind.TextArea)
            ]
        },
        new()
        {
            Slug = "diagnosticos", Title = "Diagnósticos", Singular = "diagnóstico", Subtitle = "Diagnósticos asociados a las consultas.", Endpoint = "api/Diagnosticos", IdField = "idDiagnostico", Icon = "clipboard",
            Columns = [C("idDiagnostico", "ID"), C("idConsulta", "Consulta"), C("diagnostico", "Diagnóstico"), C("descripcion", "Descripción"), C("fechaDiagnostico", "Fecha", ColumnFormat.DateTime)],
            Fields = [S("idConsulta", "Consulta", "api/Consultas", "idConsulta", "paciente", true), F("diagnostico", "Diagnóstico", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("fechaDiagnostico", "Fecha", kind: FieldKind.DateTime, defaultValue: "now")]
        },
        new()
        {
            Slug = "tratamientos", Title = "Tratamientos", Singular = "tratamiento", Subtitle = "Tratamientos indicados durante la atención clínica.", Endpoint = "api/Tratamientos", IdField = "idTratamiento", Icon = "heart",
            Columns = [C("idTratamiento", "ID"), C("idConsulta", "Consulta"), C("descripcion", "Tratamiento"), C("fechaInicio", "Inicio", ColumnFormat.Date), C("fechaFin", "Fin", ColumnFormat.Date)],
            Fields = [S("idConsulta", "Consulta", "api/Consultas", "idConsulta", "paciente", true), F("descripcion", "Descripción", true, FieldKind.TextArea), F("indicaciones", "Indicaciones", kind: FieldKind.TextArea), F("fechaInicio", "Fecha de inicio", kind: FieldKind.Date), F("fechaFin", "Fecha de fin", kind: FieldKind.Date)]
        },
        new()
        {
            Slug = "examenes", Title = "Exámenes", Singular = "examen", Subtitle = "Exámenes solicitados y resultados.", Endpoint = "api/Examenes", IdField = "idExamen", Icon = "flask",
            Columns = [C("idExamen", "ID"), C("idConsulta", "Consulta"), C("nombre", "Examen"), C("fechaExamen", "Fecha", ColumnFormat.Date), C("resultado", "Resultado")],
            Fields = [S("idConsulta", "Consulta", "api/Consultas", "idConsulta", "paciente", true), F("nombre", "Nombre del examen", true), F("fechaExamen", "Fecha", kind: FieldKind.Date), F("resultado", "Resultado", kind: FieldKind.TextArea), F("observaciones", "Observaciones", kind: FieldKind.TextArea)]
        },
        new()
        {
            Slug = "evoluciones", Title = "Evoluciones", Singular = "evolución", Subtitle = "Notas de evolución clínica del paciente.", Endpoint = "api/Evoluciones", IdField = "idEvolucion", Icon = "trend",
            Columns = [C("idEvolucion", "ID"), C("idConsulta", "Consulta"), C("fechaEvolucion", "Fecha", ColumnFormat.DateTime), C("descripcion", "Evolución")],
            Fields = [S("idConsulta", "Consulta", "api/Consultas", "idConsulta", "paciente", true), F("fechaEvolucion", "Fecha y hora", kind: FieldKind.DateTime, defaultValue: "now"), F("descripcion", "Evolución", true, FieldKind.TextArea)]
        },
        new()
        {
            Slug = "contactos-emergencia", Title = "Contactos de emergencia", Singular = "contacto", Subtitle = "Contactos de emergencia separados del expediente del paciente.", Endpoint = "api/ContactosEmergencia", IdField = "idContactoEmergencia", Icon = "phone",
            Columns = [C("idContactoEmergencia", "ID"), C("paciente", "Paciente"), C("nombre", "Contacto"), C("parentesco", "Parentesco"), C("telefono", "Teléfono")],
            Fields = [S("idPaciente", "Paciente", "api/Pacientes", "idPaciente", "nombres", true, "apellidos"), F("nombre", "Nombre", true), F("parentesco", "Parentesco", true), F("telefono", "Teléfono", true), F("direccion", "Dirección", kind: FieldKind.TextArea)]
        },
        new()
        {
            Slug = "recetas", Title = "Recetas", Singular = "receta", Subtitle = "Recetas médicas asociadas a consultas.", Endpoint = "api/Recetas", IdField = "idReceta", Icon = "file",
            Columns = [C("idReceta", "ID"), C("idConsulta", "Consulta"), C("paciente", "Paciente"), C("fechaEmision", "Emisión", ColumnFormat.DateTime), C("observaciones", "Observaciones")],
            Fields = [S("idConsulta", "Consulta", "api/Consultas", "idConsulta", "paciente", true), F("fechaEmision", "Fecha de emisión", kind: FieldKind.DateTime, defaultValue: "now"), F("observaciones", "Observaciones", kind: FieldKind.TextArea)]
        },
        new()
        {
            Slug = "detalle-receta", Title = "Medicamentos recetados", Singular = "medicamento recetado", Subtitle = "Medicamentos e indicaciones de cada receta.", Endpoint = "api/DetalleReceta", IdField = "idDetalleReceta", Icon = "pill",
            Columns = [C("idDetalleReceta", "ID"), C("idReceta", "Receta"), C("medicamento", "Medicamento"), C("cantidad", "Cantidad", ColumnFormat.Number), C("dosis", "Dosis"), C("frecuencia", "Frecuencia"), C("duracionDias", "Días", ColumnFormat.Number)],
            Fields = [S("idReceta", "Receta", "api/Recetas", "idReceta", "paciente", true), S("idMedicamento", "Medicamento", "api/Medicamentos", "idMedicamento", "nombre", true), F("cantidad", "Cantidad", true, FieldKind.Integer), F("dosis", "Dosis", true), F("frecuencia", "Frecuencia", true), F("duracionDias", "Duración (días)", true, FieldKind.Integer), F("indicaciones", "Indicaciones", kind: FieldKind.TextArea)]
        },
        new()
        {
            Slug = "habitaciones", Title = "Habitaciones", Singular = "habitación", Subtitle = "Disponibilidad, capacidad y estado de habitaciones.", Endpoint = "api/Habitaciones", IdField = "idHabitacion", Icon = "bed",
            Columns = [C("idHabitacion", "ID"), C("sucursal", "Sucursal"), C("numero", "Habitación"), C("tipoHabitacion", "Tipo"), C("cantidadCamas", "Camas", ColumnFormat.Number), C("precioPorDia", "Precio/día", ColumnFormat.Currency), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idSucursal", "Sucursal", "api/Sucursales", "idSucursal", "nombre", true), S("idTipoHabitacion", "Tipo de habitación", "api/TiposHabitacion", "idTipoHabitacion", "nombre", true), F("numero", "Número", true), F("cantidadCamas", "Cantidad de camas", true, FieldKind.Integer, defaultValue: "1"), SelectText("estado", "Estado", ["Libre", "Ocupada", "Limpieza"], "Libre")]
        },
        new()
        {
            Slug = "asignaciones-habitacion", Title = "Hospitalización", Singular = "asignación", Subtitle = "Ingresos, egresos y ocupación de camas.", Endpoint = "api/AsignacionesHabitacion", IdField = "idAsignacion", Icon = "bed",
            Columns = [C("idAsignacion", "ID"), C("paciente", "Paciente"), C("habitacion", "Habitación"), C("tipoHabitacion", "Tipo"), C("fechaIngreso", "Ingreso", ColumnFormat.DateTime), C("fechaEgreso", "Egreso", ColumnFormat.DateTime), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idPaciente", "Paciente", "api/Pacientes", "idPaciente", "nombres", true, "apellidos"), S("idHabitacion", "Habitación", "api/Habitaciones", "idHabitacion", "numero", true), F("fechaIngreso", "Fecha de ingreso", kind: FieldKind.DateTime, defaultValue: "now"), F("fechaEgreso", "Fecha de egreso", kind: FieldKind.DateTime, hiddenOnCreate: true), SelectText("estado", "Estado", ["Activa", "Finalizada"], "Activa")]
        },
        new()
        {
            Slug = "tipos-habitacion", Title = "Tipos de habitación", Singular = "tipo de habitación", Subtitle = "Tarifas por día y tipos de hospitalización.", Endpoint = "api/TiposHabitacion", IdField = "idTipoHabitacion", Icon = "building",
            Columns = [C("idTipoHabitacion", "ID"), C("nombre", "Tipo"), C("descripcion", "Descripción"), C("precioPorDia", "Precio por día", ColumnFormat.Currency), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("precioPorDia", "Precio por día", true, FieldKind.Decimal), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "categorias", Title = "Categorías", Singular = "categoría", Subtitle = "Categorías de medicamentos.", Endpoint = "api/Categorias", IdField = "idCategoria", Icon = "tag",
            Columns = [C("idCategoria", "ID"), C("nombre", "Nombre"), C("descripcion", "Descripción"), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "marcas", Title = "Marcas", Singular = "marca", Subtitle = "Marcas y laboratorios de medicamentos.", Endpoint = "api/Marcas", IdField = "idMarca", Icon = "bookmark",
            Columns = [C("idMarca", "ID"), C("nombre", "Nombre"), C("descripcion", "Descripción"), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "medicamentos", Title = "Medicamentos", Singular = "medicamento", Subtitle = "Catálogo de medicamentos y precios de venta.", Endpoint = "api/Medicamentos", IdField = "idMedicamento", Icon = "pill",
            Columns = [C("idMedicamento", "ID"), C("nombre", "Medicamento"), C("categoria", "Categoría"), C("marca", "Marca"), C("precioVenta", "Precio", ColumnFormat.Currency), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idCategoria", "Categoría", "api/Categorias", "idCategoria", "nombre", true), S("idMarca", "Marca", "api/Marcas", "idMarca", "nombre", true), F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("precioVenta", "Precio de venta", true, FieldKind.Decimal), F("imagen", "URL / referencia de imagen"), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "lotes", Title = "Lotes de medicamento", Singular = "lote", Subtitle = "Existencias por lote y fecha de vencimiento.", Endpoint = "api/LotesMedicamento", IdField = "idLoteMedicamento", Icon = "box",
            Columns = [C("idLoteMedicamento", "ID"), C("medicamento", "Medicamento"), C("numeroLote", "Lote"), C("fechaVencimiento", "Vencimiento", ColumnFormat.Date), C("cantidadDisponible", "Existencia", ColumnFormat.Number), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idMedicamento", "Medicamento", "api/Medicamentos", "idMedicamento", "nombre", true), F("numeroLote", "Número de lote", true), F("fechaVencimiento", "Fecha de vencimiento", true, FieldKind.Date), F("cantidadDisponible", "Cantidad", true, FieldKind.Integer), F("costoCompra", "Costo de compra", kind: FieldKind.Decimal), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "movimientos-inventario", Title = "Movimientos de inventario", Singular = "movimiento", Subtitle = "Entradas, salidas y ajustes del inventario.", Endpoint = "api/MovimientosInventario", IdField = "idMovimientoInventario", Icon = "arrows", CanEdit = false,
            Columns = [C("idMovimientoInventario", "ID"), C("medicamento", "Medicamento"), C("tipoMovimiento", "Tipo", ColumnFormat.Status), C("cantidad", "Cantidad", ColumnFormat.Number), C("fechaMovimiento", "Fecha", ColumnFormat.DateTime), C("observacion", "Observación")],
            Fields = [S("idLoteMedicamento", "Lote", "api/LotesMedicamento", "idLoteMedicamento", "numeroLote", true), SelectText("tipoMovimiento", "Tipo de movimiento", ["Entrada", "Salida", "Ajuste"], "Entrada"), F("cantidad", "Cantidad", true, FieldKind.Integer), F("fechaMovimiento", "Fecha", kind: FieldKind.DateTime, defaultValue: "now"), F("observacion", "Observación", kind: FieldKind.TextArea)]
        },
        new()
        {
            Slug = "ventas-farmacia", Title = "Ventas de farmacia", Singular = "venta", Subtitle = "Ventas realizadas desde farmacia.", Endpoint = "api/VentasFarmacia", IdField = "idVentaFarmacia", Icon = "cart",
            Columns = [C("idVentaFarmacia", "ID"), C("fechaVenta", "Fecha", ColumnFormat.DateTime), C("usuario", "Usuario"), C("total", "Total", ColumnFormat.Currency), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idUsuario", "Usuario", "api/Usuarios", "idUsuario", "usuarioLogin", true), F("fechaVenta", "Fecha", kind: FieldKind.DateTime, defaultValue: "now"), SelectText("estado", "Estado", ["Emitida", "Pagada", "Anulada"], "Emitida")]
        },
        new()
        {
            Slug = "detalle-venta", Title = "Detalle de ventas", Singular = "detalle de venta", Subtitle = "Medicamentos incluidos en ventas de farmacia.", Endpoint = "api/DetalleVenta", IdField = "idDetalleVenta", Icon = "receipt",
            Columns = [C("idDetalleVenta", "ID"), C("idVentaFarmacia", "Venta"), C("medicamento", "Medicamento"), C("cantidad", "Cantidad", ColumnFormat.Number), C("precioUnitario", "Precio", ColumnFormat.Currency), C("total", "Total", ColumnFormat.Currency)],
            Fields = [S("idVentaFarmacia", "Venta", "api/VentasFarmacia", "idVentaFarmacia", "idVentaFarmacia", true), S("idMedicamento", "Medicamento", "api/Medicamentos", "idMedicamento", "nombre", true), F("cantidad", "Cantidad", true, FieldKind.Integer)]
        },
        new()
        {
            Slug = "catalogo", Title = "Catálogo de facturación", Singular = "elemento de catálogo", Subtitle = "Productos y servicios disponibles para facturación.", Endpoint = "api/Catalogo", IdField = "idCatalogo", Icon = "list",
            Columns = [C("idCatalogo", "ID"), C("codigo", "Código"), C("nombre", "Nombre"), C("tipo", "Tipo", ColumnFormat.Status), C("precio", "Precio", ColumnFormat.Currency), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("codigo", "Código", true), F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), SelectText("tipo", "Tipo", ["Producto", "Servicio"], "Servicio"), F("precio", "Precio", true, FieldKind.Decimal), S("idMedicamento", "Medicamento vinculado", "api/Medicamentos", "idMedicamento", "nombre"), S("idTipoHabitacion", "Tipo de habitación vinculado", "api/TiposHabitacion", "idTipoHabitacion", "nombre"), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "facturas", Title = "Facturación", Singular = "factura", Subtitle = "Facturas de productos, servicios y hospitalización.", Endpoint = "api/Facturas", IdField = "idFactura", Icon = "receipt",
            Columns = [C("numeroFactura", "Factura"), C("paciente", "Paciente"), C("fechaFactura", "Fecha", ColumnFormat.DateTime), C("total", "Total", ColumnFormat.Currency), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idPaciente", "Paciente", "api/Pacientes", "idPaciente", "nombres", true, "apellidos"), S("idUsuario", "Usuario", "api/Usuarios", "idUsuario", "usuarioLogin", true), F("fechaFactura", "Fecha", kind: FieldKind.DateTime, defaultValue: "now"), SelectText("estado", "Estado", ["Emitida", "Pagada", "Anulada"], "Emitida")]
        },
        new()
        {
            Slug = "detalle-factura", Title = "Detalle de factura", Singular = "detalle de factura", Subtitle = "Productos y servicios incluidos en las facturas.", Endpoint = "api/DetalleFactura", IdField = "idDetalleFactura", Icon = "file",
            Columns = [C("idDetalleFactura", "ID"), C("idFactura", "Factura"), C("codigo", "Código"), C("descripcion", "Descripción"), C("cantidad", "Cantidad", ColumnFormat.Number), C("precioUnitario", "Precio", ColumnFormat.Currency), C("total", "Total", ColumnFormat.Currency)],
            Fields = [S("idFactura", "Factura", "api/Facturas", "idFactura", "numeroFactura", true), S("idCatalogo", "Producto / servicio", "api/Catalogo", "idCatalogo", "nombre", true), F("cantidad", "Cantidad", true, FieldKind.Integer, defaultValue: "1")]
        },
        new()
        {
            Slug = "usuarios", Title = "Usuarios", Singular = "usuario", Subtitle = "Cuentas de acceso al sistema.", Endpoint = "api/Usuarios", IdField = "idUsuario", Icon = "user-cog",
            Columns = [C("idUsuario", "ID"), C("usuarioLogin", "Usuario"), C("empleado", "Empleado"), C("rol", "Rol"), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [S("idEmpleado", "Empleado", "api/Empleados", "idEmpleado", "nombres", true, "apellidos"), S("idRol", "Rol", "api/Roles", "idRol", "nombre", true), F("usuarioLogin", "Usuario", true), F("password", "Contraseña", kind: FieldKind.Password, placeholder: "Mínimo 8 caracteres"), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "roles", Title = "Roles", Singular = "rol", Subtitle = "Roles y niveles de acceso.", Endpoint = "api/Roles", IdField = "idRol", Icon = "shield",
            Columns = [C("idRol", "ID"), C("nombre", "Rol"), C("descripcion", "Descripción"), C("cantidadUsuarios", "Usuarios", ColumnFormat.Number), C("cantidadPermisos", "Permisos", ColumnFormat.Number), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "permisos", Title = "Permisos por rol", Singular = "permiso", Subtitle = "Asignación de módulos y operaciones a cada rol.", Endpoint = "api/RolPermisos", IdField = "idRolPermiso", Icon = "key",
            Columns = [C("idRolPermiso", "ID"), C("rol", "Rol"), C("modulo", "Módulo"), C("operacion", "Operación")],
            Fields = [S("idRol", "Rol", "api/Roles", "idRol", "nombre", true), S("idModulo", "Módulo", "api/Modulos", "idModulo", "nombre", true), S("idOperacion", "Operación", "api/Operaciones", "idOperacion", "nombre", true)]
        },
        new()
        {
            Slug = "sucursales", Title = "Sucursales", Singular = "sucursal", Subtitle = "Sedes de la clínica.", Endpoint = "api/Sucursales", IdField = "idSucursal", Icon = "building",
            Columns = [C("idSucursal", "ID"), C("nombre", "Sucursal"), C("direccion", "Dirección"), C("telefono", "Teléfono"), C("cantidadHabitaciones", "Habitaciones", ColumnFormat.Number), C("cantidadEmpleados", "Empleados", ColumnFormat.Number), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("direccion", "Dirección", kind: FieldKind.TextArea), F("telefono", "Teléfono"), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "especialidades", Title = "Especialidades", Singular = "especialidad", Subtitle = "Especialidades médicas disponibles.", Endpoint = "api/Especialidades", IdField = "idEspecialidad", Icon = "star",
            Columns = [C("idEspecialidad", "ID"), C("nombre", "Especialidad"), C("descripcion", "Descripción"), C("cantidadEmpleados", "Empleados", ColumnFormat.Number)],
            Fields = [F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea)]
        },
        new()
        {
            Slug = "turnos", Title = "Turnos", Singular = "turno", Subtitle = "Horarios laborales del personal.", Endpoint = "api/Turnos", IdField = "idTurno", Icon = "clock",
            Columns = [C("idTurno", "ID"), C("nombre", "Turno"), C("horaInicio", "Inicio"), C("horaFin", "Fin"), C("cantidadEmpleados", "Empleados", ColumnFormat.Number), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("horaInicio", "Hora de inicio", true, FieldKind.Time), F("horaFin", "Hora de fin", true, FieldKind.Time), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "modulos", Title = "Módulos", Singular = "módulo", Subtitle = "Módulos protegidos por el sistema de permisos.", Endpoint = "api/Modulos", IdField = "idModulo", Icon = "grid",
            Columns = [C("idModulo", "ID"), C("nombre", "Módulo"), C("descripcion", "Descripción"), C("cantidadPermisos", "Permisos", ColumnFormat.Number), C("estado", "Estado", ColumnFormat.Status)],
            Fields = [F("nombre", "Nombre", true), F("descripcion", "Descripción", kind: FieldKind.TextArea), F("estado", "Activo", kind: FieldKind.Boolean, defaultValue: "true")]
        },
        new()
        {
            Slug = "operaciones", Title = "Operaciones", Singular = "operación", Subtitle = "Operaciones disponibles para el control de permisos.", Endpoint = "api/Operaciones", IdField = "idOperacion", Icon = "sliders",
            Columns = [C("idOperacion", "ID"), C("nombre", "Operación"), C("cantidadPermisos", "Permisos", ColumnFormat.Number)],
            Fields = [F("nombre", "Nombre", true)]
        }
    ];

    private static ColumnDefinition C(string key, string label, ColumnFormat format = ColumnFormat.Text)
        => new() { Key = key, Label = label, Format = format };

    private static FieldDefinition F(
        string key,
        string label,
        bool required = false,
        FieldKind kind = FieldKind.Text,
        string? placeholder = null,
        string? defaultValue = null,
        bool hiddenOnCreate = false,
        bool hiddenOnEdit = false)
        => new()
        {
            Key = key,
            Label = label,
            Required = required,
            Kind = kind,
            Placeholder = placeholder,
            DefaultValue = defaultValue,
            HiddenOnCreate = hiddenOnCreate,
            HiddenOnEdit = hiddenOnEdit
        };

    private static FieldDefinition S(
        string key,
        string label,
        string endpoint,
        string valueKey,
        string textKey,
        bool required = false,
        string? textKey2 = null)
        => new()
        {
            Key = key,
            Label = label,
            Kind = FieldKind.Select,
            Required = required,
            OptionsEndpoint = endpoint,
            OptionValueKey = valueKey,
            OptionTextKey = textKey,
            OptionTextKey2 = textKey2
        };

private static FieldDefinition SelectText(
    string key,
    string label,
    string[] values,
    string? defaultValue = null)
    => new()
    {
        Key = key,
        Label = label,
        Kind = FieldKind.Select,
        Required = true,
        OptionsEndpoint = $"$static:{string.Join('|', values)}",
        OptionValueKey = "value",
        OptionTextKey = "text",
        DefaultValue = defaultValue
    };
}
