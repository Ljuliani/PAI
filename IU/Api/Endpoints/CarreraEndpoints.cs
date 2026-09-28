using Carreras.Domain;
using Servicios;

namespace Api.Endpoints;

public static class CarreraEndpoints
{
    public static IEndpointRouteBuilder MapCarreraEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/carreras", async (
            ICarreraService carreras,
            CancellationToken cancellationToken) =>
        {
            var resultado = await carreras.ListarAsync(cancellationToken);
            return Results.Ok(resultado.Select(carrera => new
            {
                id = carrera.Id,
                nombre = carrera.Nombre,
                turno = carrera.Turno switch
                {
                    Turno.Manana => "Mañana",
                    Turno.Tarde => "Tarde",
                    Turno.Vespertino => "Vespertino",
                    _ => throw new InvalidOperationException($"El turno de la carrera {carrera.Id} no es válido.")
                }
            }));
        });

        return endpoints;
    }
}
