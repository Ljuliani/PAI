using Microsoft.Data.SqlClient;
using Servicios;

namespace Api.Endpoints;

public static class PaisEndpoints
{
    public static IEndpointRouteBuilder MapPaisEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/paises");

        group.MapGet("", async (IPaisService paises, CancellationToken cancellationToken) =>
            Results.Ok(await paises.ListarAsync(cancellationToken)));

        group.MapPost("", async (
            CrearPaisRequest solicitud,
            IPaisService paises,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await paises.AgregarAsync(solicitud.Id, solicitud.Nombre, cancellationToken);
                return Results.Created(
                    $"/api/paises/{solicitud.Id}",
                    new { id = solicitud.Id, nombre = solicitud.Nombre.Trim() });
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                loggerFactory.CreateLogger(nameof(PaisEndpoints))
                    .LogInformation(exception, "Se intentó registrar un país con un código o nombre existente.");
                return Results.Conflict(new { message = "Ya existe un país con ese código o nombre." });
            }
        }).RequireAuthorization();

        return endpoints;
    }

    public sealed record CrearPaisRequest(int Id, string Nombre);
}
