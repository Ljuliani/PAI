using AccesoaDatos.Infrastructure;
using Dapper;
using Paises.Application;
using Paises.Domain;
using System.Data;

namespace AccesoaDatos.Repositories;

public sealed class PaisRepository(IDbConnectionFactory connectionFactory) : IPaisRepository
{
    public async Task<IReadOnlyList<Pais>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT ID_Pais, Pais_Nombre
            FROM dbo.Pais
            ORDER BY Pais_Nombre;
            """;
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<PaisRow>(command);
        return rows.Select(row => new Pais
        {
            Id = row.ID_Pais,
            Nombre = row.Pais_Nombre
        }).ToList();
    }

    public async Task AgregarAsync(Pais pais, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("@ID_Pais", pais.Id, DbType.Int32);
        parameters.Add("@Pais_Nombre", pais.Nombre, DbType.String, size: 50);

        var command = new CommandDefinition(
            "dbo.Pais_Insert",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var filasAfectadas = await connection.ExecuteAsync(command);
        if (filasAfectadas != 1)
        {
            throw new InvalidOperationException("No se pudo guardar el país mediante dbo.Pais_Insert.");
        }
    }

    private sealed class PaisRow
    {
        public int ID_Pais { get; set; }
        public string Pais_Nombre { get; set; } = string.Empty;
    }
}
