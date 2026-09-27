using AccesoaDatos.Infrastructure;
using Dapper;
using Estudiantes.Application;
using Estudiantes.Domain;
using System.Data;

namespace AccesoaDatos.Repositories;

public sealed class EstudianteRepository(IDbConnectionFactory connectionFactory) : IEstudianteRepository
{
    public async Task<int> AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
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
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        var id = await connection.ExecuteScalarAsync<int?>(command);

        return id is > 0
            ? id.Value
            : throw new InvalidOperationException("El procedimiento dbo.Estudiantes_Insert no devolvió un ID_Est válido.");
    }
}
