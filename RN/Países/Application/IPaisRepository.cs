using Paises.Domain;

namespace Paises.Application;

public interface IPaisRepository
{
    Task<IReadOnlyList<Pais>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Pais pais, CancellationToken cancellationToken = default);
}
