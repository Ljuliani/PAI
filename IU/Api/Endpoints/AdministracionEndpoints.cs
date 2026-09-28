using System.Security.Claims;
using Api.Exports;
using Administracion.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Servicios;

namespace Api.Endpoints;

public static class AdministracionEndpoints
{
    public static IEndpointRouteBuilder MapAdministracionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var logger = endpoints.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(AdministracionEndpoints));

        endpoints.MapPost("/api/admin/login", async (
            CredencialesAdmin credenciales,
            IAdministracionService administracion,
            HttpContext context,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (!await administracion.AutenticarAsync(credenciales, cancellationToken))
                {
                    return Results.Unauthorized();
                }

                var identity = new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, credenciales.Usuario.Trim()), new Claim(ClaimTypes.Role, "Admin")],
                    CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4)
                    });

                return Results.Ok(new { usuario = credenciales.Usuario.Trim() });
            }
            catch (SqlException exception)
            {
                loggerFactory.CreateLogger(nameof(AdministracionEndpoints))
                    .LogError(exception, "No se pudo validar el inicio de sesión administrativo.");
                return Results.Problem(
                    "No se pudo consultar la base para validar el usuario administrador. Revisá la consola de dotnet run para ver el detalle técnico.");
            }
        });

        endpoints.MapPost("/api/admin/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        }).RequireAuthorization();

        var admin = endpoints.MapGroup("/api/admin").RequireAuthorization();

        admin.MapGet("/sesion", (ClaimsPrincipal user) =>
            Results.Ok(new { usuario = user.Identity?.Name }));

        admin.MapGet("/habilitaciones", async (
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.ListarHabilitacionesAsync(cancellationToken),
                "No se pudieron cargar las habilitaciones.",
                logger));

        admin.MapPost("/habilitaciones", async (
            NuevaHabilitacionFormulario habilitacion,
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.CrearHabilitacionAsync(habilitacion, cancellationToken),
                "No se pudo crear la habilitación.",
                logger));

        admin.MapPut("/habilitaciones/{id:int}", async (
            int id,
            EdicionHabilitacionFormulario habilitacion,
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.ActualizarHabilitacionAsync(id, habilitacion, cancellationToken),
                "No se pudo actualizar la habilitación.",
                logger));

        admin.MapDelete("/habilitaciones/{id:int}", async (
            int id,
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.EliminarHabilitacionAsync(id, cancellationToken),
                "No se pudo eliminar la habilitación.",
                logger));

        admin.MapGet("/informacion-academica", async (
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.ListarInformacionAcademicaAsync(false, cancellationToken),
                "No se pudo cargar la información académica.",
                logger));

        admin.MapPost("/informacion-academica", async (
            NuevaInformacionAcademica informacion,
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.CrearInformacionAcademicaAsync(informacion, cancellationToken),
                "No se pudo crear la información académica.",
                logger));

        admin.MapPut("/informacion-academica/{id:int}", async (
            int id,
            NuevaInformacionAcademica informacion,
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.ActualizarInformacionAcademicaAsync(id, informacion, cancellationToken),
                "No se pudo actualizar la información académica.",
                logger));

        admin.MapDelete("/informacion-academica/{id:int}", async (
            int id,
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.EliminarInformacionAcademicaAsync(id, cancellationToken),
                "No se pudo eliminar la información académica.",
                logger));

        admin.MapGet("/exportar-estudiantes", async (
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
            await EjecutarAsync(
                () => administracion.ExportarEstudiantesAsync(cancellationToken),
                "No se pudieron exportar los estudiantes.",
                logger));

        admin.MapGet("/exportar-estudiantes.xlsx", async (
            IAdministracionService administracion,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var estudiantes = await administracion.ExportarEstudiantesAsync(cancellationToken);
                var archivo = EstudianteExcelExporter.Crear(estudiantes);
                return Results.File(
                    archivo,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Estudiantes.xlsx");
            }
            catch (SqlException exception)
            {
                logger.LogError(exception, "No se pudo crear el archivo de estudiantes.");
                return Results.Problem("No se pudieron recuperar los estudiantes desde la base de datos.");
            }
        });

        endpoints.MapGet("/api/formulario/disponibilidad", async (
            IAdministracionService administracion,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var habilitacion = await administracion.ObtenerHabilitacionDisponibleAsync(cancellationToken);
                return Results.Ok(new
                {
                    disponible = habilitacion is not null,
                    habilitacion
                });
            }
            catch (SqlException exception)
            {
                loggerFactory.CreateLogger(nameof(AdministracionEndpoints))
                    .LogError(exception, "No se pudo consultar la disponibilidad del formulario.");
                return Results.Problem("No se pudo consultar la disponibilidad del formulario.");
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { message = exception.Message });
            }
        });

        return endpoints;
    }

    private static async Task<IResult> EjecutarAsync<T>(
        Func<Task<T>> operation,
        string errorMessage,
        ILogger logger)
    {
        try
        {
            var result = await operation();
            return Results.Ok(result);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        catch (SqlException exception) when (exception.Number == 50000)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627 or 547)
        {
            return Results.Conflict(new { message = "La operación entra en conflicto con datos existentes." });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "{ErrorMessage}", errorMessage);
            return Results.Problem(errorMessage);
        }
    }

    private static async Task<IResult> EjecutarAsync(
        Func<Task> operation,
        string errorMessage,
        ILogger logger)
    {
        try
        {
            await operation();
            return Results.NoContent();
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        catch (SqlException exception) when (exception.Number == 50000)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627 or 547)
        {
            return Results.Conflict(new { message = "La operación entra en conflicto con datos existentes." });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "{ErrorMessage}", errorMessage);
            return Results.Problem(errorMessage);
        }
    }
}
