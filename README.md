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

Obtiene el listado de especialidades activas. Disponible para Administradores y Pacientes.

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

Backend correspondiente al Trabajo Práctico Integrador de Desarrollo de Software 2026.
