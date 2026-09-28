using AccesoaDatos.Infrastructure;
using Dapper;
using Estudiantes.Application;
using Estudiantes.Domain;
using System.Data;

namespace AccesoaDatos.Repositories;

public sealed class EstudianteRepository(IDbConnectionFactory connectionFactory) : IEstudianteRepository
{
    public async Task<int> AgregarAsync(
        Estudiante estudiante,
        int carreraId,
        IReadOnlyList<int> informacionAcademicaIds,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string habilitacionSql = """
            SELECT ID_Hab_Form
            FROM dbo.Habilitacion_Formulario
            WHERE CONVERT(date, GETDATE()) BETWEEN Hab_Form_Fecha_Inicio AND Hab_Form_Fecha_Cierre;
            """;
        var habilitaciones = (await connection.QueryAsync<int>(
            new CommandDefinition(habilitacionSql, transaction: transaction, cancellationToken: cancellationToken))).AsList();

        if (habilitaciones.Count != 1)
        {
            throw new InvalidOperationException(habilitaciones.Count == 0
                ? "No hay una habilitación de formulario vigente para registrar inscripciones."
                : "Hay más de una habilitación vigente; no se puede determinar a cuál asociar la inscripción.");
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Est_Nom_Ape", estudiante.ApellidosYNombres, DbType.String, size: 200);
        parameters.Add("@Est_DNI", estudiante.Dni, DbType.Int32);
        parameters.Add("@Est_Correo", estudiante.Correo, DbType.AnsiString, size: 50);
        parameters.Add("@Est_Telefono", estudiante.Telefono, DbType.AnsiString, size: 20);
        parameters.Add("@Est_Fecha_Nac", estudiante.FechaNacimiento, DbType.Date);
        parameters.Add("@Est_Direccion", estudiante.Domicilio, DbType.String, size: 80);
        parameters.Add("@Est_Titulo_Sec", estudiante.TituloSecundario, DbType.AnsiString, size: 50);
        parameters.Add("@Est_Año_Egreso", estudiante.FechaEgresoSecundario, DbType.Date);
        parameters.Add("@ID_Pais", estudiante.PaisId, DbType.Int32);

        var command = new CommandDefinition(
            "dbo.Estudiantes_Insert",
            parameters,
            transaction: transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        var id = await connection.ExecuteScalarAsync<int?>(command);

        if (id is not > 0)
        {
            throw new InvalidOperationException("El procedimiento dbo.Estudiantes_Insert no devolvió un ID_Est válido.");
        }

        var inscriptionParameters = new DynamicParameters();
        inscriptionParameters.Add("@ID_Est", id.Value, DbType.Int32);
        inscriptionParameters.Add("@ID_Car", carreraId, DbType.Int32);
        inscriptionParameters.Add("@ID_Hab_Form", habilitaciones[0], DbType.Int32);
        inscriptionParameters.Add("@Mensaje", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

        var inscriptionCommand = new CommandDefinition(
            "dbo.Agregar_Inscripcion",
            inscriptionParameters,
            transaction: transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(inscriptionCommand);

        var message = inscriptionParameters.Get<string>("@Mensaje");
        if (message != "Inscripción creada exitosamente.")
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(message)
                    ? "El procedimiento dbo.Agregar_Inscripcion no devolvió un resultado."
                    : message);
        }

        const string academicIdsSql = """
            SELECT COUNT(*)
            FROM dbo.Inf_Academica
            WHERE Inf_Aca_Estado = 'HABILITADO'
              AND ID_Inf_Aca IN @Ids;
            """;
        var matchingOptions = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                academicIdsSql,
                new { Ids = informacionAcademicaIds },
                transaction: transaction,
                cancellationToken: cancellationToken));
        if (matchingOptions != informacionAcademicaIds.Count)
        {
            throw new InvalidOperationException("Una o más opciones académicas ya no están habilitadas.");
        }

        foreach (var informacionAcademicaId in informacionAcademicaIds)
        {
            var academicParameters = new DynamicParameters();
            academicParameters.Add("@ID_Inf_Aca", informacionAcademicaId, DbType.Int32);
            academicParameters.Add("@ID_Est", id.Value, DbType.Int32);
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.Inf_Academica_Est_Insert",
                academicParameters,
                transaction: transaction,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
        return id.Value;
    }
}
