# Trabajo Práctico Integrador

## Desarrollo de Software 2026

### Integrantes

| Nombre | Legajo |
|---|---:|
| Baena, Paula Melina | 60596 |
| Chirino, Carlos Iván | 60388 |
| Robles, Sisa Verónica | 60535 |
| Vicente, Juana | 60599 |

---

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
- **Turnos:** reserva, consulta, búsqueda y cancelación, con controles de consistencia y concurrencia.

---

# Configuración local del backend

## Requisitos previos

- .NET SDK 10
- Git
- Acceso al repositorio
- Opcional: herramienta `dotnet-ef` para administrar e inspeccionar migraciones.

## 1. Clonar el repositorio

Desde una terminal:

```bash
git clone https://github.com/sisarobles/dsw2026-tpi.git
cd dsw2026-tpi
git switch development
```

La rama `development` contiene la versión integrada del equipo. Si el trabajo ya fue incorporado a `main`, puede utilizarse esa rama según las indicaciones de entrega.

## 2. Configurar SQLite

La aplicación utiliza Entity Framework Core con SQLite. La configuración general se encuentra en `Dsw2026Tpi.Api/appsettings.json`; los valores locales pueden definirse mediante User Secrets o variables de entorno.

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

## 3. Configurar JWT y cuenta administrativa

La API utiliza tokens JWT para acceder a recursos protegidos. La configuración contempla los siguientes valores:

```json
{
  "Jwt": {
    "Issuer": "Dsw2026Tpi",
    "Audience": "Dsw2026Tpi",
    "ExpiresInMinutes": "60"
  },
  "AdminSeed": {
    "Email": "<EMAIL_ADMINISTRADOR>",
    "Password": "<CONTRASEÑA_SEGURA>"
  }
}
```

La clave de firma JWT se configura por separado mediante User Secrets o variables de entorno. No debe incluirse en `appsettings.json` ni subirse al repositorio.

**No subir claves ni contraseñas reales a GitHub.** Para desarrollo, utilizar User Secrets. Desde la raíz del repositorio:

```bash
dotnet user-secrets init --project Dsw2026Tpi.Api

dotnet user-secrets set "Jwt:Key" "<CLAVE_JWT_SEGURA>" --project Dsw2026Tpi.Api

dotnet user-secrets set "AdminSeed:Email" "<EMAIL_ADMINISTRADOR>" --project Dsw2026Tpi.Api

dotnet user-secrets set "AdminSeed:Password" "<CONTRASEÑA_SEGURA>" --project Dsw2026Tpi.Api
```

La clave JWT debe tener al menos 32 bytes.

Si el proyecto ya tiene configurado un `UserSecretsId`, no es necesario repetir `dotnet user-secrets init`.

La cuenta administrativa inicial se crea a partir de los valores de `AdminSeed`. Los roles utilizados por el sistema son `Administrator` y `Patient`.

### Autenticación de pacientes

El acceso de pacientes se realiza mediante correo electrónico y DNI. En el mecanismo de primer acceso, el DNI se utiliza como contraseña inicial y ASP.NET Core Identity lo almacena mediante su mecanismo de hashing, no como texto plano.

El sistema también contempla la compatibilidad con usuarios existentes que todavía no tienen una contraseña registrada, realizando la migración correspondiente durante el inicio de sesión.

**Consideración de seguridad:** el DNI es un dato personal predecible y no constituye una contraseña robusta. Para un entorno de producción se recomienda implementar el establecimiento de una contraseña personal por parte del paciente.

## 4. Configurar CORS

El backend permite configurar los orígenes desde los cuales puede acceder el frontend.

Ejemplo de configuración local:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  }
}
```

El origen configurado debe coincidir con la dirección desde la que se ejecuta el frontend. Para otros entornos, deben agregarse los orígenes correspondientes sin habilitar orígenes innecesarios.

## 5. Restaurar y compilar

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet build
```

## 6. Inicializar y actualizar la base de datos

En el entorno `Development`, la API está configurada para aplicar las migraciones de Entity Framework Core durante el inicio, antes de inicializar los datos necesarios.

El método `SeedDatabaseAsync()` se utiliza para inicializar roles y la cuenta administrativa, según la configuración vigente.

Para inspeccionar las migraciones de cada contexto:

```bash
dotnet ef migrations list --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context Dsw2026TpiDbContext

dotnet ef migrations list --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context AuthenticationDbContext
```

Para aplicar manualmente las migraciones del contexto principal:

```bash
dotnet ef database update --context Dsw2026TpiDbContext --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api
```

Para el contexto de autenticación:

```bash
dotnet ef database update --context AuthenticationDbContext --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api
```

Si se modifican entidades o configuraciones de persistencia, se deben revisar y generar las migraciones correspondientes antes de integrar los cambios.

Las migraciones deben aplicarse también en el entorno de despliegue antes de utilizar la nueva versión de la aplicación. No se debe asumir que una base de datos existente ya contiene las últimas modificaciones.

## 7. Ejecutar la API

```bash
dotnet run --project Dsw2026Tpi.Api
```

En el entorno de desarrollo, consultar la documentación Swagger/OpenAPI en la dirección que indique la aplicación.

---

# Endpoints principales

Los siguientes endpoints resumen las operaciones principales de la API. Las rutas, parámetros, respuestas y restricciones de autorización deben consultarse en Swagger/OpenAPI.

## Autenticación

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/admin/login` | Autentica al administrador mediante correo y contraseña; devuelve JWT. |
| POST | `/api/auth/patient/login` | Autentica al paciente mediante correo y DNI; contempla registro en el primer acceso. |
| POST | `/api/auth/logout` | Cierra la sesión y revoca el token JWT. |

El inicio de sesión es público. La operación de cierre de sesión requiere autenticación mediante un token válido.

## Especialidades

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/specialties` | Lista especialidades activas; permite búsqueda y paginación. |
| GET | `/api/specialties/{id}` | Consulta una especialidad activa. |
| POST | `/api/specialties` | Registra una especialidad. |
| PUT | `/api/specialties/{id}` | Actualiza una especialidad. |
| DELETE | `/api/specialties/{id}` | Realiza baja lógica. |

Las consultas de especialidades requieren autenticación. Las operaciones de creación, actualización y eliminación requieren la política de autorización de administradores.

## Médicos

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/doctors` | Lista médicos activos; permite búsqueda y paginación. |
| GET | `/api/doctors/{id}` | Consulta un médico activo. |
| GET | `/api/doctors/{id}/availabilities` | Consulta la disponibilidad del médico para el mes actual. |
| POST | `/api/doctors` | Registra un médico. |
| PUT | `/api/doctors/{id}` | Actualiza un médico. |
| DELETE | `/api/doctors/{id}` | Realiza baja lógica. |

Las consultas de médicos y sus disponibilidades requieren autenticación. Las operaciones de creación, actualización y eliminación requieren la política de autorización de administradores.

## Disponibilidades

La gestión de disponibilidades permite definir y actualizar los horarios de atención de los médicos. A partir de las reglas configuradas, se generan espacios de atención de 30 minutos.

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/availabilities` | Crea reglas de disponibilidad y genera slots para el mes en curso. |
| PUT | `/api/availabilities` | Actualiza las reglas de disponibilidad del mes. |
| GET | `/api/availabilities/{doctorId}/slots` | Consulta los espacios de atención de un médico. |

La API también permite consultar las disponibilidades del médico mediante:

`GET /api/doctors/{id}/availabilities`

La gestión de disponibilidades contempla:

- Creación de disponibilidades para un médico.
- Consulta de las disponibilidades del mes actual.
- Consulta de espacios de atención.
- Actualización de las reglas de disponibilidad del mes.
- Generación de espacios de atención para fechas válidas.
- Exclusión de días feriados.
- Validación de rangos horarios y solapamientos.
- Protección de los turnos reservados al modificar disponibilidades.

Al actualizar las disponibilidades, los turnos reservados de fechas actuales o futuras impiden eliminar las reglas correspondientes. Los turnos reservados históricos se conservan para mantener el registro de las atenciones.

## Turnos

La API permite gestionar las reservas de atención médica. Las operaciones contemplan:

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/appointments` | Reserva un turno. |
| GET | `/api/appointments/patient` | Consulta los turnos del paciente autenticado. |
| DELETE | `/api/appointments/{id}` | Cancela un turno. |
| GET | `/api/appointments` | Lista turnos de una fecha; operación administrativa. |
| GET | `/api/appointments/search` | Busca turnos mediante filtros; operación administrativa. |

La creación de un turno recibe el identificador del espacio de disponibilidad (`availabilitySlotId`). La reserva se realiza únicamente si el espacio continúa disponible.

La cancelación modifica el estado del turno y libera el espacio correspondiente cuando la operación es válida.

La API contempla controles de consistencia para evitar reservas duplicadas y conflictos cuando varias solicitudes intentan reservar el mismo espacio.

Las rutas, parámetros, respuestas y permisos específicos deben consultarse en Swagger/OpenAPI.

---

# Autenticación y autorización

La API utiliza JWT para autenticar las solicitudes a los recursos protegidos.

Los roles principales son:

- `Administrator`: permite acceder a las operaciones de administración.
- `Patient`: permite acceder a las operaciones correspondientes a pacientes.

Para acceder a un endpoint protegido, se debe enviar el token en el encabezado HTTP:

```http
Authorization: Bearer <TOKEN>
```

Las respuestas de autenticación devuelven el rol en mayúsculas (`ADMINISTRATOR` o `PATIENT`), mientras que los roles internos utilizados para las políticas de autorización mantienen sus identificadores configurados en Identity.

El endpoint de cierre de sesión revoca el token asociado a la sesión. Para utilizarlo, se debe enviar un token válido.

# Manejo de errores

La API centraliza el tratamiento de excepciones mediante un middleware de manejo de errores.

Las respuestas de error utilizan una estructura uniforme:

```json
{
  "errorCode": "ERROR_CODE",
  "message": "Descripción del error.",
  "details": []
}
```

Cuando corresponde, `details` incluye información adicional sobre los campos que produjeron el error.

Entre los códigos HTTP utilizados se encuentran:

| Código HTTP | Significado |
|---|---|
| 400 | Solicitud inválida o errores de validación. |
| 401 | Falta de autenticación o credenciales inválidas. |
| 403 | El usuario autenticado no tiene permisos suficientes. |
| 404 | La entidad solicitada no existe o no está disponible. |
| 409 | Conflicto con el estado actual o con una regla de negocio. |
| 429 | Se excedió el límite de solicitudes permitido. |
| 500 | Error interno no contemplado específicamente. |

Los errores de validación del modelo también se devuelven utilizando el formato uniforme de la API.

# Limitación de solicitudes (Rate Limiting)

La API implementa limitación de solicitudes para reducir el abuso de los endpoints y controlar la frecuencia de acceso.

Los límites se configuran en `appsettings.json` y pueden sobrescribirse por entorno mediante archivos de configuración específicos.

Los grupos de políticas contemplados son:

- `AdminLogin`: solicitudes de inicio de sesión de administradores.
- `PatientLogin`: solicitudes de inicio de sesión de pacientes.
- `Appointments`: operaciones relacionadas con turnos.
- `General`: límite global de solicitudes.

En la configuración base, los límites se establecen por ventana de un minuto:

| Política | Límite base |
|---|---:|
| AdminLogin | 5 solicitudes |
| PatientLogin | 10 solicitudes |
| Appointments | 5 solicitudes |
| General | 100 solicitudes |

En `Development` se utilizan límites superiores para facilitar las pruebas y el desarrollo local. Estos valores no reemplazan los límites de la configuración base ni deben trasladarse automáticamente a producción.

Cuando se supera un límite, la API responde con HTTP `429 Too Many Requests`.

# Pruebas

El proyecto incluye pruebas automatizadas desarrolladas con xUnit y NSubstitute.

Las pruebas permiten verificar el comportamiento de los servicios y las reglas de negocio, incluyendo:

- Autenticación de administradores y pacientes.
- Validaciones de datos.
- Gestión de disponibilidades.
- Generación y consulta de espacios de atención.
- Actualización de disponibilidades y conservación de reservas históricas.
- Reserva y cancelación de turnos.
- Manejo de conflictos y errores.
- Consistencia ante operaciones concurrentes.

Para ejecutar las pruebas desde la raíz del repositorio:

```bash
dotnet test
```

Para compilar la solución:

```bash
dotnet build
```

**Estado de verificación:** en la última ejecución local se obtuvo una compilación exitosa y 79 pruebas aprobadas, sin errores ni pruebas omitidas. La compilación finalizó sin advertencias y las 79 pruebas automatizadas finalizaron correctamente.

# Consideraciones adicionales

- Las bajas de especialidades, médicos, disponibilidades y otras entidades administradas se realizan mediante baja lógica cuando corresponde.
- Las restricciones de integridad de la base de datos complementan las validaciones realizadas por los servicios.
- La reserva de turnos utiliza operaciones de persistencia que controlan el estado del espacio para evitar reservas simultáneas.
- Las credenciales, claves JWT y cadenas de conexión locales no deben incluirse en el repositorio.
- Antes de desplegar una nueva versión, deben verificarse las migraciones pendientes y la configuración del entorno.
- Los turnos reservados históricos se conservan al actualizar las disponibilidades, mientras que las reservas actuales o futuras protegen las reglas correspondientes.

# Documentación de la API

La documentación interactiva de los endpoints se encuentra disponible mediante Swagger/OpenAPI cuando se ejecuta la API en el entorno correspondiente.

Swagger constituye la referencia para consultar las rutas completas, métodos HTTP, parámetros, esquemas de solicitud y respuesta, y requisitos de autorización.

---

**Trabajo Práctico Integrador — Desarrollo de Software 2026**