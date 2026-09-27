using Carreras.Domain;

namespace Carreras.Application;

public interface ICarreraRepository
{
    Task<Carrera?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Carrera>> ListarPorTurnoAsync(Turno turno, CancellationToken cancellationToken = default);
}
