using Estudiantes.Domain;

namespace Estudiantes.Application;

public interface IInformacionAcademicaRepository
{
    Task<IReadOnlyList<InformacionAcademica>> ListarHabilitadasAsync(
        CancellationToken cancellationToken = default);
}
