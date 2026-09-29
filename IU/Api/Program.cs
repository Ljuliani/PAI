using AccesoaDatos;
using Servicios;
using Api.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;

Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
Directory.CreateDirectory(webRootPath);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = webRootPath
});
var connectionString = builder.Configuration.GetConnectionString("Sql")
    ?? throw new InvalidOperationException(
        "Falta configurar ConnectionStrings:Sql. En desarrollo local, usá User Secrets; "
        + "en el servidor publicado, configurá la variable de entorno ConnectionStrings__Sql.");

builder.Services.AddServicios();
builder.Services.AddAccesoDatos(connectionString);
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PAI.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(4);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/paises")
        && context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/administracion/");
        return;
    }

    await next();
});
app.UseStaticFiles();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/estudiantes/"));
app.MapPaisEndpoints();
app.MapCarreraEndpoints();
app.MapEstudianteEndpoints();
app.MapAdministracionEndpoints();

await app.RunAsync();
