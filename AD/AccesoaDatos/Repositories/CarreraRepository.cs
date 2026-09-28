using AccesoaDatos.Infrastructure;
using Carreras.Application;
using Carreras.Domain;
using Dapper;

namespace AccesoaDatos.Repositories;

public sealed class CarreraRepository(IDbConnectionFactory connectionFactory) : ICarreraRepository
{
    public async Task<IReadOnlyList<Carrera>> ListarAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ID_Car AS Id, Car_Nom AS Nombre, Turno
            FROM dbo.Carreras
            ORDER BY Car_Nom;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<CarreraRow>(command);

        return rows.Select(Map).ToList();
    }

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
            Turno = MapTurno(reader.GetString(2))
        };

    private static Carrera Map(CarreraRow row) =>
        new()
        {
            Id = row.Id,
            Nombre = row.Nombre,
            Turno = MapTurno(row.Turno)
        };

    private static Turno MapTurno(string value) =>
        value switch
        {
            "Mañana" or "MAÑANA" => Turno.Manana,
            "Tarde" or "TARDE" => Turno.Tarde,
            "Vespertino" or "VESPERTINO" => Turno.Vespertino,
            _ => throw new InvalidOperationException($"El turno '{value}' almacenado en Carreras no es válido.")
        };

    private sealed class CarreraRow
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
    }
}
