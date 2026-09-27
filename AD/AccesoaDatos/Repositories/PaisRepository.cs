using AccesoaDatos.Infrastructure;
using Dapper;
using Paises.Application;
using Paises.Domain;

namespace AccesoaDatos.Repositories;

public sealed class PaisRepository(IDbConnectionFactory connectionFactory) : IPaisRepository
{
    public async Task<IReadOnlyList<Pais>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ID_PAIS, Pais_Nombre FROM dbo.Pais ORDER BY Pais_Nombre;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var paises = new List<Pais>();
        while (await reader.ReadAsync(cancellationToken))
        {
            paises.Add(new Pais
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1)
            });
        }

        return paises;
    }

    public async Task AgregarAsync(Pais pais, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.Pais (ID_PAIS, Pais_Nombre)
            VALUES (@Id, @Nombre);
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, pais, cancellationToken: cancellationToken);
        var filasAfectadas = await connection.ExecuteAsync(command);
        if (filasAfectadas != 1)
        {
            throw new InvalidOperationException("No se pudo guardar el país.");
        }
    }
}
