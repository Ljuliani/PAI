using System.Data.Common;

namespace AccesoaDatos.Infrastructure;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
