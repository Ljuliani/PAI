namespace Servicios;

public interface IEstudianteService
{
    Task<int> AgregarAsync(
        SolicitudEstudiante solicitud,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Estudiantes.Domain.InformacionAcademica>> ListarInformacionAcademicaAsync(
        CancellationToken cancellationToken = default);
}
