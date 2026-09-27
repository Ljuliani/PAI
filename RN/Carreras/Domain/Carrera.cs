namespace Carreras.Domain;

public sealed class Carrera
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Turno Turno { get; set; }
}
