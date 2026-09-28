using Estudiantes.Domain;

namespace Estudiantes.Application;

public interface IEstudianteRepository
{
    Task<int> AgregarAsync(
        Estudiante estudiante,
        int carreraId,
        IReadOnlyList<int> informacionAcademicaIds,
        CancellationToken cancellationToken = default);
}
