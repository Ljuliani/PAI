using Microsoft.Data.SqlClient;
using Servicios;

namespace Api.Endpoints;

public static class EstudianteEndpoints
{
    public static IEndpointRouteBuilder MapEstudianteEndpoints(this IEndpointRouteBuilder endpoints)
    {
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
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                loggerFactory.CreateLogger(nameof(EstudianteEndpoints))
                    .LogInformation(exception, "Conflicto de unicidad al guardar el estudiante.");
                return Results.Conflict(new { message = "Ya existe un estudiante con ese DNI y correo." });
            }
        });

        return endpoints;
    }
}
