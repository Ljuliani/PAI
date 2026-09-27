namespace Servicios;

public interface IEstudianteService
{
    Task<int> AgregarAsync(
        SolicitudEstudiante solicitud,
        CancellationToken cancellationToken = default);
}
