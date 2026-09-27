using Estudiantes.Domain;

namespace Estudiantes.Application;

public interface IEstudianteRepository
{
    Task<int> AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default);
}
