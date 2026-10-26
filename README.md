# Trabajo Práctico Integrador

## Desarrollo de Software 2026

### Integrantes

| Nombre | Legajo |
|---------|---------|
| Baena, Paula Melina | 60596 | 
| Gutierrez, Maia | 60348 | 
| Herrera Barboza, Lucrecia Jazmín | 60754 | 
|Robles, Sisa Verónica | 60535 | 

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

## Estado del proyecto

---

## Configuración local del backend

### Requisitos previos

- .NET SDK 10.
- Git.
- Acceso al repositorio del proyecto.

### 1. Obtener el proyecto

Clonar el repositorio y entrar en la carpeta raíz:

```bash
git clone https://github.com/sisarobles/dsw2026-tpi.git
cd dsw2026-tpi
```

Cambiar a la rama de trabajo que corresponda si se necesita probar cambios todavía no integrados.

### 2. Configurar secretos locales

Los archivos `appsettings.Development.json` y `appsettings.Local.json` no se versionan porque pueden contener información sensible.

Desde la raíz del repositorio, inicializar User Secrets para el proyecto de API:

```bash
dotnet user-secrets init --project Dsw2026Tpi.Api
```

Configurar los valores que la aplicación necesita para el entorno local, según las claves utilizadas por el proyecto. Por ejemplo:

```bash
dotnet user-secrets set "Jwt:Key" "<CLAVE_JWT_LOCAL>" --project Dsw2026Tpi.Api
dotnet user-secrets set "AdminSeed:Email" "<EMAIL_ADMIN_LOCAL>" --project Dsw2026Tpi.Api
dotnet user-secrets set "AdminSeed:Password" "<CONTRASEÑA_ADMIN_LOCAL>" --project Dsw2026Tpi.Api
```

Reemplazar los valores de ejemplo por valores locales. No subir contraseñas ni claves reales al repositorio.

Si la aplicación requiere otros parámetros locales, configurarlos usando las claves definidas en el código.

### 3. Restaurar dependencias y compilar

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet build
```

### 4. Crear y actualizar las bases de datos

En el entorno `Development`, la API ejecuta las migraciones de Entity Framework Core al iniciar, antes de cargar los datos iniciales.

Se utilizan dos contextos:

- `Dsw2026TpiDbContext`: persistencia principal de la aplicación.
- `AuthenticationDbContext`: persistencia de autenticación e identidad.

Las bases SQLite son locales y no deben agregarse al repositorio. Para comprobar las migraciones del contexto principal:

```bash
dotnet ef migrations list --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context Dsw2026TpiDbContext
```

Para el contexto de autenticación:

```bash
dotnet ef migrations list --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context AuthenticationDbContext
```

Antes de iniciar la aplicación, verificar que las migraciones estén sincronizadas con los modelos.

### 5. Ejecutar la API

```bash
dotnet run --project Dsw2026Tpi.Api
```

En desarrollo, Swagger permite consultar y probar los endpoints de la API.

### 6. Consideraciones para el equipo

- Cada integrante debe configurar sus propios secretos locales.
- No compartir ni versionar archivos con contraseñas o claves JWT.
- No versionar archivos de bases de datos SQLite (`*.db`).
- Si se modifican entidades o configuraciones de persistencia, coordinar y generar las migraciones correspondientes antes de integrar los cambios.
- Para probar una instalación limpia, utilizar una base local nueva y comprobar que las migraciones y la carga inicial se ejecuten correctamente.

Backend correspondiente al Trabajo Práctico Integrador de Desarrollo de Software 2026.
