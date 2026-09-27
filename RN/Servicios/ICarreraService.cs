using Carreras.Domain;

namespace Servicios;

public interface ICarreraService
{
    Task<IReadOnlyList<Carrera>> ListarPorTurnoAsync(
        Turno turno,
        CancellationToken cancellationToken = default);
}
