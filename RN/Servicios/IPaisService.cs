using Paises.Domain;

namespace Servicios;

public interface IPaisService
{
    Task<IReadOnlyList<Pais>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(int id, string nombre, CancellationToken cancellationToken = default);
}
