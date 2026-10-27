# Trabajo Práctico Integrador

## Desarrollo de Software 2026

### Integrantes

| Nombre | Legajo |
|---------|---------|
| Baena, Paula Melina | 60596 | 
| Chirino, Carlos Iván | 60388 |  
|Robles, Sisa Verónica | 60535 | 
| Vicente, Juana | 60599 |
---

## Descripción

Este proyecto corresponde al Trabajo Práctico Integrador de la materia Desarrollo de Software.

Se desarrolló una API REST para la gestión de turnos médicos, permitiendo administrar médicos, especialidades, disponibilidades y reservas de turnos mediante autenticación basada en JWT.

---

## Tecnologías utilizadas

- C#
- ASP.NET Core
- .NET 10
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- JWT
- Swagger / OpenAPI
- xUnit
- NSubstitute

---
## Configuración del entorno

La aplicación utiliza ASP.NET Core y Entity Framework Core con SQLite para la persistencia de datos.

La configuración general se encuentra en `Dsw2026Tpi.Api/appsettings.json`, mientras que la configuración específica del entorno de desarrollo puede definirse en `Dsw2026Tpi.Api/appsettings.Development.json`.

### Configuración de SQLite

La cadena de conexión utilizada para desarrollo tiene el siguiente formato:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=dsw2026_local.db"
  }
}
```

La aplicación utiliza SQLite como motor de base de datos.

Durante el inicio se ejecuta `SeedDatabaseAsync()`, que realiza las tareas de inicialización definidas por el proyecto.

### Configuración de JWT

La API utiliza autenticación basada en tokens JWT para controlar el acceso a los recursos protegidos.

La configuración debe proporcionar los siguientes valores:

```json
{
  "Jwt": {
    "Key": "<clave-segura>",
    "Issuer": "Dsw2026Tpi.Api",
    "Audience": "Dsw2026Tpi.ApiUsers",
    "ExpiresInMinutes": 60
  }
}
```

La propiedad `Key` debe contener una clave secreta segura, utilizada para firmar los tokens.

Por seguridad, se recomienda configurar las credenciales mediante **User Secrets** o variables de entorno, evitando publicar claves reales y contraseñas en el repositorio.

Para establecer la clave mediante User Secrets, desde el directorio del proyecto API se puede ejecutar:

```bash
dotnet user-secrets set "Jwt:Key" "<clave-segura>"
```

Para utilizar User Secrets, el proyecto debe tener configurado un `UserSecretsId` y ejecutarse en el entorno `Development`.

### Inicialización de roles y administrador

Durante el inicio de la aplicación se ejecuta el método `SeedDatabaseAsync()`, encargado de inicializar los roles y la cuenta administrativa del sistema.

El proceso realiza las siguientes operaciones:

- Verifica la existencia de los roles `Administrator` y `Patient`, y los crea cuando es necesario.
- Comprueba si existe un administrador con el correo configurado.
- Si no existe, intenta crear la cuenta administrativa y asignarle el rol `Administrator`.

Los datos de la cuenta inicial se obtienen de la sección `AdminSeed`:

```json
{
  "AdminSeed": {
    "Email": "<correo-administrador>",
    "Password": "<contraseña-segura>"
  }
}
```

Por seguridad, se recomienda configurar estas credenciales mediante User Secrets o variables de entorno y evitar incluir contraseñas reales en el repositorio.

**Importante:** este proceso inicializa roles y usuarios, pero no aplica migraciones de Entity Framework Core. La base de datos y las tablas correspondientes deben encontrarse disponibles antes de ejecutar la inicialización.

### Configuración mediante User Secrets

Desde la carpeta `Dsw2026Tpi.Api`, se pueden configurar las credenciales ejecutando:

```bash
dotnet user-secrets set "AdminSeed:Email" "<correo-administrador>"
dotnet user-secrets set "AdminSeed:Password" "<contraseña-segura>"
```

Estos comandos requieren que el proyecto tenga configurado un `UserSecretsId` y que se utilice el entorno de desarrollo.

---
## Persistencia de datos

El proyecto utiliza **Entity Framework Core con SQLite** para almacenar la información.

Se configuran dos contextos de base de datos:

- `Dsw2026TpiDbContext`: gestiona la persistencia de las entidades del sistema de turnos médicos.
- `AuthenticationDbContext`: gestiona los usuarios, roles y demás información de autenticación.

Ambos contextos utilizan la cadena de conexión `DefaultConnection`, definida en la configuración de la aplicación.

### Inicialización de datos

El sistema dispone de mecanismos para inicializar roles y una cuenta administrativa.

Durante el inicio de la aplicación, `SeedDatabaseAsync()` verifica la existencia de los roles `Administrator` y `Patient`, y crea una cuenta administrativa inicial si no existe.

Adicionalmente, el contexto de autenticación tiene configurado un mecanismo de inicialización de roles a partir del archivo `Sources/roles.json`.

### Migraciones

La aplicación utiliza migraciones de Entity Framework Core para administrar la estructura de la base de datos.

Antes de ejecutar el proyecto por primera vez, debe verificarse que las migraciones correspondientes a ambos contextos se hayan aplicado.

La configuración de persistencia revisada no ejecuta migraciones automáticamente durante el inicio de la aplicación.

---
# Endpoints

## Autenticación

### POST /api/auth/admin/login

Autentica un administrador mediante correo electrónico y contraseña. Devuelve un token JWT para acceder a los recursos protegidos.

### POST /api/auth/patient/login

Autentica a un paciente utilizando correo electrónico y DNI. Si es el primer acceso, el paciente se registra automáticamente y se genera un token JWT.

---

## Especialidades

### GET /api/specialties

Obtiene el listado de especialidades activas. Disponible para Administradores.

Permite paginación y búsqueda por nombre.

### GET /api/specialties/{id}

Obtiene la información de una especialidad médica activa a partir de su identificador.

Solo disponible para administradores.

### POST /api/specialties

Registra una nueva especialidad médica.

Solo disponible para Administradores.

### PUT /api/specialties/{id}

Actualiza una especialidad existente.

Solo disponible para Administradores.

### DELETE /api/specialties/{id}

Realiza el borrado lógico de una especialidad.

Solo disponible para Administradores.

---

## Médicos

### GET /api/doctors

Obtiene el listado de médicos activos.

Permite paginación y búsqueda por nombre.

### GET /api/doctors/{id}

Obtiene la información de un médico activo a partir de su identificador, incluyendo los datos de su especialidad cuando tiene una asignada.

Solo disponible para administradores.

### GET /api/doctors/{id}/availabilities

Obtiene la disponibilidad del médico para el mes actual.

### POST /api/doctors

Registra un nuevo médico.

Solo disponible para Administradores.

### PUT /api/doctors/{id}

Actualiza un médico existente.

Solo disponible para Administradores.

### DELETE /api/doctors/{id}

Realiza el borrado lógico de un médico.

Solo disponible para Administradores.

---

## Disponibilidades

### POST /api/availabilities

Genera automáticamente los turnos disponibles del médico para el mes en curso.

Solo disponible para Administradores.

### PUT /api/availabilities

Actualiza la disponibilidad mensual del médico manteniendo las reservas existentes.

Solo disponible para Administradores.

---

## Turnos

### POST /api/appointments

Permite a un paciente reservar un turno.

### GET /api/appointments/patient

Obtiene los turnos reservados por el paciente.

### DELETE /api/appointments/{id}

Cancela un turno reservado.

### GET /api/appointments

Lista los turnos de una fecha determinada.

Solo disponible para Administradores.

### GET /api/appointments/search

Permite realizar búsquedas avanzadas de turnos mediante distintos filtros.

Solo disponible para Administradores.

---

## Documentación

Al ejecutar la aplicación en modo desarrollo, la documentación de la API estará disponible mediante Swagger.

---
## Pruebas unitarias

El proyecto incluye pruebas unitarias desarrolladas con **xUnit** y **NSubstitute**, organizadas en el proyecto `Dsw2026Tpi.Tests`.

Las pruebas permiten verificar reglas de negocio del dominio y el comportamiento de los servicios sin depender de una base de datos real.

### Pruebas implementadas

- **AppointmentServiceDeleteTests:** cancelación de turnos y liberación de horarios reservados.
- **AvailabilityServiceUpdateTests:** actualización de disponibilidades y validación de horarios reservados.
- **DoctorServiceGetByIdTests:** consulta de médicos activos, inexistentes e inactivos.
- **SpecialityServiceGetByIdTests:** consulta de especialidades activas, inexistentes e inactivas.
- **AppointmentTests:** creación de citas, cancelación y prevención de cancelaciones duplicadas.

### Ejecución de las pruebas

Desde Visual Studio:

1. Abrir la solución del proyecto.
2. Seleccionar **Prueba → Explorador de pruebas**.
3. Ejecutar todas las pruebas.

También pueden ejecutarse desde una terminal ubicada en la raíz del repositorio:

```bash
dotnet test
```

Actualmente se cuenta con **13 pruebas unitarias aprobadas**.

---

## Estado del proyecto

Backend correspondiente al Trabajo Práctico Integrador de Desarrollo de Software 2026.
