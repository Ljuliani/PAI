namespace Administracion.Domain;

public sealed record CredencialesAdmin(string Usuario, string Contraseña);

public sealed record HabilitacionFormulario(
    int Id,
    int Año,
    DateOnly FechaInicio,
    DateOnly FechaCierre,
    string Estado);

public sealed record NuevaHabilitacionFormulario(
    int Año,
    DateOnly FechaInicio,
    DateOnly FechaCierre);

public sealed record EdicionHabilitacionFormulario(
    DateOnly FechaInicio,
    DateOnly FechaCierre);

public sealed record InformacionAcademicaAdmin(
    int Id,
    string Descripcion,
    DateOnly Fecha,
    string Estado);

public sealed record NuevaInformacionAcademica(
    string Descripcion,
    DateOnly Fecha,
    string Estado);

public sealed record EstudianteExportacion(
    string CorreoElectronico,
    string CarreraInscripta,
    string NombreApellido,
    int Dni,
    string Telefono,
    DateTime FechaNacimiento,
    int EdadActual,
    string Direccion,
    string Posee,
    string TituloSecundario,
    DateTime? AñoEgreso);
