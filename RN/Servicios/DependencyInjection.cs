using Microsoft.Extensions.DependencyInjection;

namespace Servicios;

public static class DependencyInjection
{
    public static IServiceCollection AddServicios(this IServiceCollection services)
    {
        services.AddScoped<IEstudianteService, EstudianteService>();
        services.AddScoped<ICarreraService, CarreraService>();
        services.AddScoped<IPaisService, PaisService>();
        services.AddScoped<IAdministracionService, AdministracionService>();
        return services;
    }
}
