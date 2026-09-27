using Paises.Application;
using Paises.Domain;

namespace Servicios;

public sealed class PaisService(IPaisRepository paises) : IPaisService
{
    public Task<IReadOnlyList<Pais>> ListarAsync(CancellationToken cancellationToken = default) =>
        paises.ListarAsync(cancellationToken);

    public Task AgregarAsync(int id, string nombre, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "El código del país debe ser un entero positivo.");
        }

        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 50)
        {
            throw new ArgumentException("El nombre del país es obligatorio y admite hasta 50 caracteres.", nameof(nombre));
        }

        return paises.AgregarAsync(new Pais { Id = id, Nombre = nombre.Trim() }, cancellationToken);
    }
}
