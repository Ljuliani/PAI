using AccesoaDatos.Infrastructure;
using Administracion.Application;
using Administracion.Domain;
using Dapper;
using Estudiantes.Application;
using Estudiantes.Domain;
using System.Data;

namespace AccesoaDatos.Repositories;

public sealed class AdministracionRepository(IDbConnectionFactory connectionFactory) :
    IAdministracionRepository,
    IInformacionAcademicaRepository
{
    public async Task<bool> AutenticarAsync(
        CredencialesAdmin credenciales,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("@Admin_Nom_Usuario", credenciales.Usuario.Trim(), DbType.AnsiString, size: 30);
        parameters.Add("@Admin_Contra", credenciales.Contraseña, DbType.AnsiString, size: 30);
        parameters.Add("@Mensaje", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.Logueo_Admin",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return string.Equals(parameters.Get<string>("@Mensaje"), "Logueo exitoso.", StringComparison.Ordinal);
    }

    public async Task<IReadOnlyList<HabilitacionFormulario>> ListarHabilitacionesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT ID_Hab_Form, Hab_Form_Año, Hab_Form_Fecha_Inicio, Hab_Form_Fecha_Cierre
            FROM dbo.Habilitacion_Formulario
            ORDER BY Hab_Form_Año DESC;
            """;
        var rows = await connection.QueryAsync<HabilitacionRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return rows.Select(row =>
        {
            var inicio = DateOnly.FromDateTime(row.Hab_Form_Fecha_Inicio);
            var cierre = DateOnly.FromDateTime(row.Hab_Form_Fecha_Cierre);
            var estado = hoy < inicio ? "Programada" : hoy > cierre ? "Cerrada" : "Vigente";
            return new HabilitacionFormulario(row.ID_Hab_Form, row.Hab_Form_Año, inicio, cierre, estado);
        }).ToList();
    }

    public async Task CrearHabilitacionAsync(
        NuevaHabilitacionFormulario habilitacion,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("@Hab_Form_Año", habilitacion.Año, DbType.Int32);
        parameters.Add("@Hab_Form_Fecha_Inicio", habilitacion.FechaInicio, DbType.Date);
        parameters.Add("@Hab_Form_Fecha_Cierre", habilitacion.FechaCierre, DbType.Date);

        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.sp_Insert_Habilitacion_Formulario",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task ActualizarHabilitacionAsync(
        int id,
        EdicionHabilitacionFormulario habilitacion,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("@ID_Hab_Form", id, DbType.Int32);
        parameters.Add("@Hab_Form_Fecha_Inicio", habilitacion.FechaInicio, DbType.Date);
        parameters.Add("@Hab_Form_Fecha_Cierre", habilitacion.FechaCierre, DbType.Date);

        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.sp_Update_Habilitacion_Formulario",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task EliminarHabilitacionAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.sp_Delete_Habilitacion_Formulario",
            new { ID_Hab_Form = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<HabilitacionFormulario?> ObtenerHabilitacionDisponibleAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT ID_Hab_Form, Hab_Form_Año, Hab_Form_Fecha_Inicio, Hab_Form_Fecha_Cierre
            FROM dbo.Habilitacion_Formulario
            WHERE CONVERT(date, GETDATE()) BETWEEN Hab_Form_Fecha_Inicio AND Hab_Form_Fecha_Cierre;
            """;
        var rows = (await connection.QueryAsync<HabilitacionRow>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken))).AsList();

        if (rows.Count > 1)
        {
            throw new InvalidOperationException("Hay más de una habilitación vigente.");
        }

        var row = rows.SingleOrDefault();
        return row is null
            ? null
            : new HabilitacionFormulario(
                row.ID_Hab_Form,
                row.Hab_Form_Año,
                DateOnly.FromDateTime(row.Hab_Form_Fecha_Inicio),
                DateOnly.FromDateTime(row.Hab_Form_Fecha_Cierre),
                "Vigente");
    }

    public async Task<IReadOnlyList<InformacionAcademicaAdmin>> ListarInformacionAcademicaAsync(
        bool soloHabilitadas,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT ID_Inf_Aca, Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado
            FROM dbo.Inf_Academica
            WHERE @SoloHabilitadas = 0 OR Inf_Aca_Estado = 'HABILITADO'
            ORDER BY Inf_Aca_Descripcion;
            """;
        var rows = await connection.QueryAsync<InformacionAcademicaRow>(new CommandDefinition(
            sql,
            new { SoloHabilitadas = soloHabilitadas },
            cancellationToken: cancellationToken));

        return rows.Select(ToAdminModel).ToList();
    }

    public async Task<IReadOnlyList<InformacionAcademica>> ListarHabilitadasAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT ID_Inf_Aca, Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado
            FROM dbo.Inf_Academica
            WHERE Inf_Aca_Estado = 'HABILITADO'
            ORDER BY Inf_Aca_Descripcion;
            """;
        var rows = await connection.QueryAsync<InformacionAcademicaRow>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken));

        return rows.Select(row => new InformacionAcademica(
            row.ID_Inf_Aca,
            row.Inf_Aca_Descripcion,
            DateOnly.FromDateTime(row.Inf_Aca_Fecha),
            row.Inf_Aca_Estado)).ToList();
    }

    public async Task CrearInformacionAcademicaAsync(
        NuevaInformacionAcademica informacion,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var parameters = CrearParametrosInformacion(informacion);
        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.Inf_Academica_Insert",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task ActualizarInformacionAcademicaAsync(
        int id,
        NuevaInformacionAcademica informacion,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("@ID_Inf_Aca", id, DbType.Int32);
        parameters.Add("@Nueva_Descripcion", informacion.Descripcion.Trim(), DbType.AnsiString, size: 100);
        parameters.Add("@Nueva_Fecha", informacion.Fecha, DbType.Date);
        parameters.Add("@Nuevo_Estado", informacion.Estado, DbType.AnsiString, size: 15);

        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.Inf_Academica_Update",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task EliminarInformacionAcademicaAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.Inf_Academica_Delete",
            new { ID_Inf_Aca = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<EstudianteExportacion>> ExportarEstudiantesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        const string sql = """
            DECLARE @FechaReferencia date = DATEFROMPARTS(YEAR(GETDATE()), 6, 30);

            SELECT
                e.Est_Correo AS CorreoElectronico,
                CONCAT(c.Car_Nom, ' (', c.Turno, ')') AS CarreraInscripta,
                e.Est_Nom_Ape AS NombreApellido,
                e.Est_DNI AS Dni,
                e.Est_Telefono AS Telefono,
                e.Est_Fecha_Nac AS FechaNacimiento,
                DATEDIFF(YEAR, e.Est_Fecha_Nac, @FechaReferencia)
                    - CASE
                        WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.Est_Fecha_Nac, @FechaReferencia), e.Est_Fecha_Nac) > @FechaReferencia
                            THEN 1
                        ELSE 0
                      END AS EdadActual,
                e.Est_Direccion AS Direccion,
                ia.Inf_Aca_Descripcion AS Posee,
                e.Est_Titulo_Sec AS TituloSecundario,
                e.Est_Año_Egreso AS [AñoEgreso]
            FROM dbo.Inscripcion AS i
            INNER JOIN dbo.Estudiantes AS e ON e.ID_Est = i.ID_Est
            INNER JOIN dbo.Carreras AS c ON c.ID_Car = i.ID_Car
            LEFT JOIN dbo.Inf_Academica_Est AS iae ON iae.ID_Est = i.ID_Est
            LEFT JOIN dbo.Inf_Academica AS ia ON ia.ID_Inf_Aca = iae.ID_Inf_Aca
            ORDER BY c.Car_Nom, c.Turno, e.Est_Nom_Ape, ia.Inf_Aca_Descripcion;
            """;
        var rows = await connection.QueryAsync<EstudianteExportacion>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken));

        return rows
            .GroupBy(row => new { row.Dni, row.CarreraInscripta })
            .Select(group =>
            {
                var student = group.First();
                return student with
                {
                    Posee = string.Join(", ", group
                        .Select(row => row.Posee)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.OrdinalIgnoreCase))
                };
            })
            .ToList();
    }

    private static DynamicParameters CrearParametrosInformacion(NuevaInformacionAcademica informacion)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Inf_Aca_Descripcion", informacion.Descripcion.Trim(), DbType.AnsiString, size: 50);
        parameters.Add("@Inf_Aca_Fecha", informacion.Fecha, DbType.Date);
        parameters.Add("@Inf_Aca_Estado", informacion.Estado, DbType.AnsiString, size: 15);
        return parameters;
    }

    private static InformacionAcademicaAdmin ToAdminModel(InformacionAcademicaRow row) =>
        new(
            row.ID_Inf_Aca,
            row.Inf_Aca_Descripcion,
            DateOnly.FromDateTime(row.Inf_Aca_Fecha),
            row.Inf_Aca_Estado);

    private sealed class HabilitacionRow
    {
        public int ID_Hab_Form { get; set; }
        public int Hab_Form_Año { get; set; }
        public DateTime Hab_Form_Fecha_Inicio { get; set; }
        public DateTime Hab_Form_Fecha_Cierre { get; set; }
    }

    private sealed class InformacionAcademicaRow
    {
        public int ID_Inf_Aca { get; set; }
        public string Inf_Aca_Descripcion { get; set; } = string.Empty;
        public DateTime Inf_Aca_Fecha { get; set; }
        public string Inf_Aca_Estado { get; set; } = string.Empty;
    }
}
