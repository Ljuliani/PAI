using Administracion.Application;
using Administracion.Domain;

namespace Servicios;

public sealed class AdministracionService(IAdministracionRepository administracion) : IAdministracionService
{
    public Task<bool> AutenticarAsync(CredencialesAdmin credenciales, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credenciales);
        if (string.IsNullOrWhiteSpace(credenciales.Usuario) ||
            credenciales.Usuario.Length > 30 ||
            string.IsNullOrWhiteSpace(credenciales.Contraseña) ||
            credenciales.Contraseña.Length > 30)
        {
            return Task.FromResult(false);
        }

        return administracion.AutenticarAsync(credenciales, cancellationToken);
    }

    public Task<IReadOnlyList<HabilitacionFormulario>> ListarHabilitacionesAsync(CancellationToken cancellationToken = default) =>
        administracion.ListarHabilitacionesAsync(cancellationToken);

    public Task CrearHabilitacionAsync(NuevaHabilitacionFormulario habilitacion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(habilitacion);
        if (habilitacion.Año <= 0 ||
            habilitacion.FechaInicio >= habilitacion.FechaCierre)
        {
            throw new ArgumentException("El año y el rango de fechas de habilitación no son válidos.");
        }

        return administracion.CrearHabilitacionAsync(habilitacion, cancellationToken);
    }

    public Task ActualizarHabilitacionAsync(int id, EdicionHabilitacionFormulario habilitacion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(habilitacion);
        if (id <= 0 ||
            habilitacion.FechaInicio >= habilitacion.FechaCierre)
        {
            throw new ArgumentException("El rango de fechas de habilitación no es válido.");
        }

        return administracion.ActualizarHabilitacionAsync(id, habilitacion, cancellationToken);
    }

    public Task EliminarHabilitacionAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return administracion.EliminarHabilitacionAsync(id, cancellationToken);
    }

    public Task<HabilitacionFormulario?> ObtenerHabilitacionDisponibleAsync(CancellationToken cancellationToken = default) =>
        administracion.ObtenerHabilitacionDisponibleAsync(cancellationToken);

    public Task<IReadOnlyList<InformacionAcademicaAdmin>> ListarInformacionAcademicaAsync(
        bool soloHabilitadas,
        CancellationToken cancellationToken = default) =>
        administracion.ListarInformacionAcademicaAsync(soloHabilitadas, cancellationToken);

    public Task CrearInformacionAcademicaAsync(NuevaInformacionAcademica informacion, CancellationToken cancellationToken = default)
    {
        ValidarInformacion(informacion);
        return administracion.CrearInformacionAcademicaAsync(informacion, cancellationToken);
    }

    public Task ActualizarInformacionAcademicaAsync(int id, NuevaInformacionAcademica informacion, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ValidarInformacion(informacion);
        return administracion.ActualizarInformacionAcademicaAsync(id, informacion, cancellationToken);
    }

    public Task EliminarInformacionAcademicaAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return administracion.EliminarInformacionAcademicaAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<EstudianteExportacion>> ExportarEstudiantesAsync(CancellationToken cancellationToken = default) =>
        administracion.ExportarEstudiantesAsync(cancellationToken);

    private static void ValidarInformacion(NuevaInformacionAcademica informacion)
    {
        ArgumentNullException.ThrowIfNull(informacion);
        if (string.IsNullOrWhiteSpace(informacion.Descripcion) ||
            informacion.Descripcion.Trim().Length > 50 ||
            informacion.Estado is not ("HABILITADO" or "DESHABILITADO"))
        {
            throw new ArgumentException("La descripción o el estado de información académica no son válidos.");
        }
    }
}
