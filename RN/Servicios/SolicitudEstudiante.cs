namespace Servicios;

public sealed record SolicitudEstudiante(
    int Dni,
    string ApellidosYNombres,
    DateOnly FechaNacimiento,
    string Correo,
    string Domicilio,
    string Telefono,
    DateOnly? FechaEgresoSecundario,
    string TituloSecundario,
    int PaisId);
