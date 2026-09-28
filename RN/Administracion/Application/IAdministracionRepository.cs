using Administracion.Domain;

namespace Administracion.Application;

public interface IAdministracionRepository
{
    Task<bool> AutenticarAsync(CredencialesAdmin credenciales, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HabilitacionFormulario>> ListarHabilitacionesAsync(CancellationToken cancellationToken = default);
    Task CrearHabilitacionAsync(NuevaHabilitacionFormulario habilitacion, CancellationToken cancellationToken = default);
    Task ActualizarHabilitacionAsync(int id, EdicionHabilitacionFormulario habilitacion, CancellationToken cancellationToken = default);
    Task EliminarHabilitacionAsync(int id, CancellationToken cancellationToken = default);
    Task<HabilitacionFormulario?> ObtenerHabilitacionDisponibleAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InformacionAcademicaAdmin>> ListarInformacionAcademicaAsync(bool soloHabilitadas, CancellationToken cancellationToken = default);
    Task CrearInformacionAcademicaAsync(NuevaInformacionAcademica informacion, CancellationToken cancellationToken = default);
    Task ActualizarInformacionAcademicaAsync(int id, NuevaInformacionAcademica informacion, CancellationToken cancellationToken = default);
    Task EliminarInformacionAcademicaAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EstudianteExportacion>> ExportarEstudiantesAsync(CancellationToken cancellationToken = default);
}
