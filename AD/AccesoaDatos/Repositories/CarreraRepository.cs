using AccesoaDatos.Infrastructure;
using Carreras.Application;
using Carreras.Domain;

namespace AccesoaDatos.Repositories;

public sealed class CarreraRepository(IDbConnectionFactory connectionFactory) : ICarreraRepository
{
    public async Task<Carrera?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ID_Car, Car_Nom, Turno
            FROM dbo.Carreras
            WHERE ID_Car = @Id;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<Carrera>> ListarPorTurnoAsync(
        Turno turno,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ID_Car, Car_Nom, Turno
            FROM dbo.Carreras
            WHERE Turno = @Turno
            ORDER BY Car_Nom;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Turno";
        parameter.Value = turno switch
        {
            Turno.Manana => "Mañana",
            Turno.Tarde => "Tarde",
            Turno.Vespertino => "Vespertino",
            _ => throw new ArgumentOutOfRangeException(nameof(turno))
        };
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var carreras = new List<Carrera>();
        while (await reader.ReadAsync(cancellationToken))
        {
            carreras.Add(Map(reader));
        }

        return carreras;
    }

    private static Carrera Map(System.Data.Common.DbDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Turno = reader.GetString(2) switch
            {
                "Mañana" => Turno.Manana,
                "Tarde" => Turno.Tarde,
                "Vespertino" => Turno.Vespertino,
                var value => throw new InvalidOperationException($"El turno '{value}' almacenado en Carreras no es válido.")
            }
        };
}
