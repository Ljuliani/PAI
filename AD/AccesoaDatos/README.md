# Acceso a datos

La persistencia usa Dapper como micro-ORM y SQL Server/Azure SQL como motor. La base y las tablas se crean por separado con el script SQL de la arquitectura de datos; Dapper no crea ni modifica el esquema. El repositorio de estudiantes ejecuta el procedimiento almacenado `dbo.Estudiantes_Insert`. La cadena de conexión se inyecta desde el host; no debe guardarse en el repositorio ni en archivos versionados.

Las bibliotecas `RN/Estudiantes`, `RN/Países` y `RN/Carreras` mantienen separados sus modelos. La tabla `Pais` usa `ID_PAIS INT PRIMARY KEY` sin `IDENTITY`; los códigos los define la carga/administración del catálogo, no Dapper. El alta de estudiante recibe ese `PaisId`; el procedimiento valida que exista y resuelve la unicidad compuesta DNI-correo. El servicio retorna el `ID_Est` generado o existente por ese procedimiento.

La definición vigente del país es:

```sql
CREATE TABLE Pais (
    ID_PAIS INT PRIMARY KEY,
    Pais_Nombre VARCHAR(50) UNIQUE NOT NULL
);
```

1. Crear la base y las tablas con el script SQL de la arquitectura de datos.
2. Ejecutar `Database/Estudiantes_Insert.sql` contra esa base para instalar o actualizar el procedimiento almacenado.
3. Proveer la cadena de conexión mediante la configuración segura del entorno de ejecución (por ejemplo, secretos del proveedor de nube o variables de entorno).
4. Registrar las capas en el host:

```csharp
services.AddServicios();
services.AddAccesoDatos(configuration.GetConnectionString("Sql")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Sql."));
```

La cadena `ConnectionStrings:Sql` debe incluir el servidor, la base, la autenticación y el cifrado exigido por el proveedor. Para Azure SQL, por ejemplo, configurá `Encrypt=True` y `TrustServerCertificate=False`; evitá desactivar la validación del certificado.

El host HTTP se encuentra en `IU/Api`. El proyecto ya tiene `UserSecretsId` configurado. Para guardar la cadena localmente (fuera del repositorio) y ejecutar:

```sh
dotnet user-secrets set "ConnectionStrings:Sql" "<cadena de conexión de Somee>" --project IU/Api/Api.csproj
dotnet run --project IU/Api/Api.csproj
```

Reemplazá el texto entre `<...>` por la cadena completa en tu terminal local. No la agregues a este README, a `appsettings.json` ni a Git. La variable de entorno equivalente es `ConnectionStrings__Sql`.

La pantalla de administración de países queda en `/paises/`; el formulario de estudiantes, en `/estudiantes/`. La API ofrece `GET/POST /api/paises` y `POST /api/estudiantes`. Las pantallas se sirven desde el mismo origen que la API, por lo que no requieren configuración CORS.

La pantalla de países es una interfaz administrativa, pero todavía no tiene autenticación/autorización de usuarios. No publiques el endpoint de alta de países en producción hasta protegerlo con el mecanismo de identidad y permisos elegido para el despliegue.
