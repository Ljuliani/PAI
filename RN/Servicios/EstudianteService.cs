using Estudiantes.Application;
using Estudiantes.Domain;

namespace Servicios;

public sealed class EstudianteService(
    IEstudianteRepository estudiantes,
    IInformacionAcademicaRepository informacionAcademica) : IEstudianteService
{
    public async Task<int> AgregarAsync(
        SolicitudEstudiante solicitud,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        ValidarSolicitud(solicitud);

        var estudiante = new Estudiante
        {
            Dni = solicitud.Dni,
            ApellidosYNombres = solicitud.ApellidosYNombres.Trim(),
            FechaNacimiento = solicitud.FechaNacimiento,
            Correo = solicitud.Correo.Trim(),
            Domicilio = solicitud.Domicilio.Trim(),
            Telefono = solicitud.Telefono.Trim(),
            FechaEgresoSecundario = solicitud.FechaEgresoSecundario,
            TituloSecundario = solicitud.TituloSecundario.Trim(),
            PaisId = solicitud.PaisId
        };

        var informacionAcademicaIds = solicitud.InformacionAcademicaIds ?? [];
        if (informacionAcademicaIds.Count == 0 ||
            informacionAcademicaIds.Any(id => id <= 0) ||
            informacionAcademicaIds.Distinct().Count() != informacionAcademicaIds.Count)
        {
            throw new ArgumentException("Seleccioná al menos una opción de información académica válida.", nameof(solicitud));
        }

        return await estudiantes.AgregarAsync(
            estudiante,
            solicitud.CarreraId,
            informacionAcademicaIds,
            cancellationToken);
    }

    public Task<IReadOnlyList<InformacionAcademica>> ListarInformacionAcademicaAsync(
        CancellationToken cancellationToken = default) =>
        informacionAcademica.ListarHabilitadasAsync(cancellationToken);

    private static void ValidarSolicitud(SolicitudEstudiante solicitud)
    {
        if (solicitud.Dni <= 0 ||
            solicitud.PaisId <= 0 ||
            solicitud.CarreraId <= 0 ||
            string.IsNullOrWhiteSpace(solicitud.ApellidosYNombres) ||
            string.IsNullOrWhiteSpace(solicitud.Correo) ||
            string.IsNullOrWhiteSpace(solicitud.Domicilio) ||
            string.IsNullOrWhiteSpace(solicitud.Telefono) ||
            string.IsNullOrWhiteSpace(solicitud.TituloSecundario))
        {
            throw new ArgumentException(
                "DNI, país, carrera y todos los datos personales requeridos deben ser válidos.",
                nameof(solicitud));
        }

        if (solicitud.ApellidosYNombres.Length > 200 ||
            solicitud.Correo.Length > 50 ||
            solicitud.Telefono.Length > 20 ||
            solicitud.Domicilio.Length > 80 ||
            solicitud.TituloSecundario.Length > 50)
        {
            throw new ArgumentException(
                "Uno o más datos superan el máximo permitido por la base de datos.",
                nameof(solicitud));
        }

        if (solicitud.FechaNacimiento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("La fecha de nacimiento no puede ser futura.", nameof(solicitud));
        }
    }
}
