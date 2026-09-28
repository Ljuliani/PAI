# Acceso a datos

La persistencia de estudiantes y países usa Dapper como micro-ORM sobre SQL Server/Azure SQL. La capa de acceso a datos no crea ni modifica el esquema: delega las escrituras en procedimientos almacenados ya existentes y mapea los resultados devueltos por la base a objetos del dominio.

En el alcance actual, los objetos que se persisten son `Estudiante` y `Pais`. Sus repositorios no contienen sentencias directas `INSERT`, `UPDATE` o `DELETE`; delegan las escrituras en procedimientos almacenados existentes. No se crean tablas desde AD.

- `dbo.Estudiantes_Insert` para crear o recuperar un estudiante y devolver el `ID_Est`.
- `dbo.Pais_listados` para listar países.
- `dbo.Pais_Insert` para registrar un país.

El formulario carga carreras desde `dbo.Carreras` y opciones académicas habilitadas desde `dbo.Inf_Academica`. El alta crea o recupera al estudiante, ejecuta `dbo.Agregar_Inscripcion` y registra las opciones seleccionadas mediante `dbo.Inf_Academica_Est_Insert`, dentro de una transacción. Solo acepta una habilitación cuyas fechas incluyan el día actual; la selección académica debe seguir habilitada al guardar.

La capa de administración delega las escrituras en procedimientos existentes para el inicio de sesión (`dbo.Logueo_Admin`), el mantenimiento de habilitaciones y de información académica. Los países, las habilitaciones y las opciones académicas se leen con consultas parametrizadas porque la base configurada no tiene procedimientos de listado disponibles para esas entidades. La exportación consulta las tablas existentes directamente y genera un archivo `.xlsx` en la API, sin dependencia de servicios CDN del navegador. El cambio de habilitación solo actualiza fechas, conforme a la firma de `sp_Update_Habilitacion_Formulario` compartida; la inscripción está abierta si la fecha actual está dentro de una única ventana de fechas. No se crea ni modifica el esquema desde esta aplicación.

La exportación usa las columnas que devuelve `dbo.SP_ExportacionDelExcel`. El procedimiento compartido no devuelve una fecha de inscripción, por lo que no se inventa ese dato; las opciones `Posee` asociadas a una misma inscripción se agrupan en el objeto exportado.

La cadena de conexión se inyecta desde el host; no debe guardarse en el repositorio ni en archivos versionados.

1. Configurar la base de datos y los procedimientos almacenados fuera de esta capa; AD asume que ya están creados.
2. Proveer la cadena de conexión mediante la configuración segura del entorno de ejecución (por ejemplo, secretos del proveedor de nube o variables de entorno).
3. Registrar las capas en el host:

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

La administración queda en `/administracion/`; el formulario de estudiantes, en `/estudiantes/`, y la pantalla de países, en `/paises/`. La API expone, entre otras, `GET /api/carreras`, `GET /api/informacion-academica`, `POST /api/estudiantes`, `GET /api/formulario/disponibilidad` y rutas `/api/admin/*` protegidas con una cookie de sesión obtenida a través de `dbo.Logueo_Admin`. Las pantallas se sirven desde el mismo origen que la API, por lo que no requieren configuración CORS.

El alta de países (`POST /api/paises`) también requiere la sesión administrativa.
