namespace Estudiantes.Domain;

public sealed class Estudiante
{
    public int Id { get; set; }
    public int Dni { get; set; }
    public string ApellidosYNombres { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Domicilio { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateOnly? FechaEgresoSecundario { get; set; }
    public string TituloSecundario { get; set; } = string.Empty;
    public int PaisId { get; set; }
}
