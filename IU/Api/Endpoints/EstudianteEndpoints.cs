using Microsoft.Data.SqlClient;
using Servicios;

namespace Api.Endpoints;

public static class EstudianteEndpoints
{
    public static IEndpointRouteBuilder MapEstudianteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/informacion-academica", async (
            IEstudianteService estudiantes,
            CancellationToken cancellationToken) =>
        {
            var informacion = await estudiantes.ListarInformacionAcademicaAsync(cancellationToken);
            return Results.Ok(informacion);
        });

        endpoints.MapPost("/api/estudiantes", async (
            SolicitudEstudiante solicitud,
            IEstudianteService estudiantes,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var id = await estudiantes.AgregarAsync(solicitud, cancellationToken);
                return Results.Ok(new { idEstudiante = id });
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (SqlException exception) when (exception.Number is >= 50001 and <= 50012)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (SqlException exception) when (exception.Number == 50000)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                loggerFactory.CreateLogger(nameof(EstudianteEndpoints))
                    .LogInformation(exception, "Conflicto de unicidad al guardar el estudiante.");
                return Results.Conflict(new { message = "Ya existe el estudiante o su inscripción ya fue registrada." });
            }
            catch (SqlException exception)
            {
                loggerFactory.CreateLogger(nameof(EstudianteEndpoints))
                    .LogError(exception, "Error de base de datos al registrar al estudiante y su inscripción.");
                return Results.Problem("No se pudo registrar al estudiante en este momento.");
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { message = exception.Message });
            }
        });

        return endpoints;
    }
}
