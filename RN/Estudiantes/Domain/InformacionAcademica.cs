namespace Estudiantes.Domain;

public sealed record InformacionAcademica(
    int Id,
    string Descripcion,
    DateOnly Fecha,
    string Estado);
