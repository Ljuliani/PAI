using Carreras.Application;
using Carreras.Domain;
using Estudiantes.Application;

namespace Servicios;

public sealed class CarreraService(ICarreraRepository carreras) : ICarreraService
{
    public Task<IReadOnlyList<Carrera>> ListarPorTurnoAsync(
        Turno turno,
        CancellationToken cancellationToken = default) =>
        carreras.ListarPorTurnoAsync(turno, cancellationToken);
}
