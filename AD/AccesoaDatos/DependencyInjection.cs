using AccesoaDatos.Infrastructure;
using AccesoaDatos.Repositories;
using Administracion.Application;
using Carreras.Application;
using Estudiantes.Application;
using Microsoft.Extensions.DependencyInjection;
using Paises.Application;

namespace AccesoaDatos;

public static class DependencyInjection
{
    public static IServiceCollection AddAccesoDatos(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddScoped<IEstudianteRepository, EstudianteRepository>();
        services.AddScoped<ICarreraRepository, CarreraRepository>();
        services.AddScoped<IPaisRepository, PaisRepository>();
        services.AddScoped<AdministracionRepository>();
        services.AddScoped<Estudiantes.Application.IInformacionAcademicaRepository>(
            serviceProvider => serviceProvider.GetRequiredService<AdministracionRepository>());
        services.AddScoped<IAdministracionRepository>(
            serviceProvider => serviceProvider.GetRequiredService<AdministracionRepository>());
        return services;
    }
}
