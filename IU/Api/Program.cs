using AccesoaDatos;
using Servicios;
using Api.Endpoints;

Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
Directory.CreateDirectory(webRootPath);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = webRootPath
});
var connectionString = builder.Configuration.GetConnectionString("Sql")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Sql.");

builder.Services.AddServicios();
builder.Services.AddAccesoDatos(connectionString);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/estudiantes/"));
app.MapPaisEndpoints();
app.MapEstudianteEndpoints();

await app.RunAsync();
