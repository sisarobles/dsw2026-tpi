# Trabajo Práctico Integrador

## Desarrollo de Software 2026

### Integrantes

| Nombre | Legajo |
|---|---|
| Baena, Paula Melina | 60596 |
| Chirino, Carlos Iván | 60388 |
| Robles, Sisa Verónica | 60535 |
| Vicente, Juana | 60599 |

## Descripción

Trabajo Práctico Integrador de Desarrollo de Software 2026. Se desarrolla una API REST para la gestión de turnos médicos, con administración de especialidades, médicos, disponibilidades y reservas, y autenticación mediante JWT.

## Tecnologías utilizadas

- C# y .NET 10
- ASP.NET Core Web API
- Entity Framework Core y SQLite
- ASP.NET Core Identity y JWT
- Swagger / OpenAPI
- xUnit y NSubstitute

## Funcionalidades principales

- **Autenticación:** acceso de administradores y pacientes mediante los mecanismos definidos por la API.
- **Especialidades:** alta, consulta, actualización y baja lógica.
- **Médicos:** alta, consulta, actualización y baja lógica, con asociación a especialidades.
- **Disponibilidades:** definición y actualización de horarios por médico; generación de espacios de atención de 30 minutos y validación de reglas de negocio.
- **Turnos:** reserva, consulta, búsqueda y cancelación, con controles de consistencia.

## Configuración local del backend

### Requisitos previos

- .NET SDK 10
- Git
- Acceso al repositorio
- Opcional: herramienta `dotnet-ef` para inspeccionar migraciones

### 1. Clonar el repositorio

```bash
git clone https://github.com/sisarobles/dsw2026-tpi.git
cd dsw2026-tpi
git switch development
```

La rama `development` contiene la versión integrada del equipo. Si el trabajo ya fue incorporado a `main`, puede utilizarse esa rama según las indicaciones de entrega.

### 2. Configurar SQLite

La aplicación utiliza Entity Framework Core con SQLite. La configuración general se encuentra en `Dsw2026Tpi.Api/appsettings.json`; los valores locales pueden definirse en la configuración de desarrollo o mediante User Secrets.

Ejemplo de cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=dsw2026_local.db"
  }
}
```

El sistema utiliza dos contextos:

- `Dsw2026TpiDbContext`: entidades del sistema de turnos médicos.
- `AuthenticationDbContext`: usuarios, roles y autenticación.

Ambos contextos utilizan la cadena `DefaultConnection`. La base de datos SQLite es local y no debe versionarse en el repositorio.

### 3. Configurar JWT y cuenta administrativa

La API utiliza tokens JWT para acceder a recursos protegidos. La configuración contempla valores como los siguientes:

```json
{
  "Jwt": {
    "Key": "<CLAVE_JWT_SEGURA>",
    "Issuer": "Dsw2026Tpi.Api",
    "Audience": "Dsw2026Tpi.ApiUsers",
    "ExpiresInMinutes": 60
  },
  "AdminSeed": {
    "Email": "<EMAIL_ADMINISTRADOR>",
    "Password": "<CONTRASEÑA_SEGURA>"
  }
}
```

**No subir claves ni contraseñas reales a GitHub.** Para desarrollo, utilizar User Secrets o variables de entorno. Desde la raíz del repositorio:

```bash
dotnet user-secrets init --project Dsw2026Tpi.Api
dotnet user-secrets set "Jwt:Key" "<CLAVE_JWT_SEGURA>" --project Dsw2026Tpi.Api
dotnet user-secrets set "AdminSeed:Email" "<EMAIL_ADMINISTRADOR>" --project Dsw2026Tpi.Api
dotnet user-secrets set "AdminSeed:Password" "<CONTRASEÑA_SEGURA>" --project Dsw2026Tpi.Api
```

Si el proyecto ya tiene configurado un `UserSecretsId`, no es necesario repetir `dotnet user-secrets init`.

### 4. Restaurar y compilar

```bash
dotnet restore
dotnet build
```

### 5. Inicializar la base de datos

En el entorno `Development`, la API está configurada para aplicar las migraciones de Entity Framework Core durante el inicio, antes de inicializar los datos necesarios. El método `SeedDatabaseAsync()` se utiliza para inicializar roles y la cuenta administrativa, según la configuración vigente.

Los roles utilizados son `Administrator` y `Patient`. El administrador inicial se configura mediante `AdminSeed`.

Para inspeccionar las migraciones de cada contexto, si se dispone de `dotnet-ef`:

```bash
dotnet ef migrations list --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context Dsw2026TpiDbContext
dotnet ef migrations list --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context AuthenticationDbContext
```

Si se modifican entidades o configuraciones de persistencia, se deben revisar y generar las migraciones correspondientes antes de integrar los cambios.

### 6. Ejecutar la API

```bash
dotnet run --project Dsw2026Tpi.Api
```

En el entorno de desarrollo, consultar la documentación Swagger/OpenAPI en la dirección que indique la aplicación.

## Endpoints principales

Los siguientes endpoints resumen las operaciones documentadas. Las restricciones de autorización y los parámetros concretos deben consultarse en Swagger/OpenAPI.

### Autenticación

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/admin/login` | Autentica al administrador mediante correo y contraseña; devuelve JWT. |
| POST | `/api/auth/patient/login` | Autentica al paciente mediante correo y DNI; contempla registro en el primer acceso. |

### Especialidades

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/specialties` | Lista especialidades activas; permite búsqueda y paginación. |
| GET | `/api/specialties/{id}` | Consulta una especialidad activa. |
| POST | `/api/specialties` | Registra una especialidad. |
| PUT | `/api/specialties/{id}` | Actualiza una especialidad. |
| DELETE | `/api/specialties/{id}` | Realiza baja lógica. |

Las operaciones de especialidades están destinadas a administradores según la documentación existente del proyecto.

### Médicos

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/doctors` | Lista médicos activos; permite búsqueda y paginación. |
| GET | `/api/doctors/{id}` | Consulta un médico activo. |
| GET | `/api/doctors/{id}/availabilities` | Consulta la disponibilidad del médico para el mes actual. |
| POST | `/api/doctors` | Registra un médico. |
| PUT | `/api/doctors/{id}` | Actualiza un médico. |
| DELETE | `/api/doctors/{id}` | Realiza baja lógica. |

Las operaciones de administración de médicos requieren los permisos correspondientes.

### Disponibilidades

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/availabilities` | Crea reglas de disponibilidad y genera slots para el mes en curso. |
| PUT | `/api/availabilities` | Actualiza la disponibilidad, respetando las restricciones sobre reservas existentes. |

Estas operaciones están destinadas a administradores.

### Turnos

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/appointments` | Reserva un turno. |
| GET | `/api/appointments/patient` | Consulta los turnos del paciente autenticado. |
| DELETE | `/api/appointments/{id}` | Cancela un turno. |
| GET | `/api/appointments` | Lista turnos de una fecha; operación administrativa. |
| GET | `/api/appointments/search` | Busca turnos mediante filtros; operación administrativa. |

## Pruebas automatizadas

El proyecto incluye pruebas con **xUnit** y **NSubstitute**, ubicadas en `Dsw2026Tpi.Tests`. Verifican reglas de negocio y comportamientos de servicios, utilizando dobles de prueba para aislar dependencias cuando corresponde.

### Cobertura funcional de las pruebas

- **Disponibilidades — creación:** médico inexistente o inactivo, horarios superpuestos, rango horario inválido, generación de slots de 30 minutos, feriados y prevención de slots que excedan el horario configurado.
- **Disponibilidades — actualización:** validaciones asociadas a cambios de horarios y reservas existentes.
- **Turnos — creación, cancelación y persistencia:** comportamientos de los servicios y consistencia de reservas.
- **Dominio de turnos:** creación, cancelación y reglas para evitar cancelaciones duplicadas.
- **Médicos y especialidades:** consultas de entidades activas, inexistentes o inactivas.
- **API:** pruebas del manejo de excepciones mediante middleware.

Entre los archivos de pruebas se encuentran `AvailabilityServiceCreateTests.cs`, `AvailabilityServiceUpdateTests.cs`, `AppointmentServiceCreateTests.cs`, `AppointmentServiceDeleteTests.cs`, `AppointmentPersistenceTests.cs`, `AppointmentTests.cs`, `DoctorServiceGetByIdTests.cs`, `SpecialityServiceGetByIdTests.cs` y `ExceptionHandlingMiddlewareTests.cs`.

### Cómo ejecutar las pruebas

Desde la raíz del repositorio:

```bash
dotnet test Dsw2026Tpi.Tests
```

También pueden ejecutarse desde el Explorador de pruebas de Visual Studio.

**Última ejecución local verificada:** 36 pruebas correctas, 0 fallidas y 0 omitidas. Compilación exitosa con 5 advertencias de nulabilidad (CS8618, CS8602 y CS8620). Estas advertencias no impidieron la ejecución, pero quedan identificadas para revisión técnica.

El conjunto de pruebas incluye una regresión para comprobar que los slots de 30 minutos no sobrepasen el final del rango horario de disponibilidad.

## Estado del proyecto

A la fecha de la última validación local:

- Backend ASP.NET Core integrado en `development`.
- Persistencia con Entity Framework Core y SQLite.
- Autenticación mediante ASP.NET Core Identity y JWT.
- Funcionalidades de médicos, especialidades, disponibilidades y turnos implementadas.
- Cambios de refactorización de disponibilidades y consistencia de turnos integrados por el equipo.
- **36 pruebas automatizadas aprobadas**.
- **5 advertencias de nulabilidad pendientes de revisión**.

La ejecución satisfactoria de pruebas automatizadas no sustituye las verificaciones manuales de endpoints, autenticación y despliegue en un entorno limpio.

## Consideraciones para el equipo

- Cada integrante debe configurar sus propios secretos locales.
- No versionar claves JWT, contraseñas ni archivos de bases de datos SQLite (`*.db`).
- Trabajar en ramas de funcionalidad o documentación y crear Pull Requests hacia `development`.
- Coordinar los cambios de entidades y migraciones antes de integrarlos.
- Para comprobar una instalación limpia, usar una base local nueva y verificar la aplicación de migraciones y la inicialización de roles y administrador.

---

**Trabajo Práctico Integrador — Desarrollo de Software 2026**
