using System.Globalization;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly JwtService _jwtService;
    private readonly IPersistence _persistence;
    private readonly ILogService _logService;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        ISignInService signInManager,
        JwtService jwtService,
        IPersistence persistence,
        ILogService logService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _persistence = persistence;
        _logService = logService;
    }

    public async Task<LoginAdminModel.Response> LoginAdmin(
        LoginAdminModel.Request request)
    {
        if (!request.Email.IsEmailValid())
            throw new AuthenticationException();

        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new AuthenticationException();

        var passwordIsValid = await _signInManager.CheckPassword(
            user,
            request.Password);

        if (!passwordIsValid)
        {
            await _logService.LogAsync(
                "Auth",
                "LOGIN_ADMIN_FAILED",
                $"Intento de login fallido para {request.Email}",
                LogLevel.Warning);

            throw new AuthenticationException();
        }

        if (user.Deleted)
        {
            await _logService.LogAsync(
                "Auth",
                "LOGIN_ADMIN_FAILED",
                $"Intento de login de usuario eliminado: {request.Email}",
                LogLevel.Warning);

            throw new AuthenticationException();
        }

        var isAdministrator = await _userManager.IsInRoleAsync(
            user,
            Roles.Administrator);

        if (!isAdministrator)
        {
            await _logService.LogAsync(
                "Auth",
                "LOGIN_ADMIN_FAILED",
                $"Intento de acceso administrativo sin rol de administrador: {request.Email}",
                LogLevel.Warning);

            throw new AuthenticationException();
        }

        var token = _jwtService.GenerateToken(
            user.Id,
            user.UserName!,
            Roles.Administrator);

        await _logService.LogAsync(
            "Auth",
            "LOGIN_ADMIN_SUCCESS",
            $"Login exitoso para {request.Email}");

        return new LoginAdminModel.Response(
            token,
            Roles.Administrator.ToUpperInvariant());
    }

    public async Task<LoginPatientModel.Response> LoginPatient(
        LoginPatientModel.Request request)
    {
        if (!request.Email.IsEmailValid())
            throw new AuthenticationException();

        if (request.Dni < 1_000_000 || request.Dni > 99_999_999)
        {
            throw new ValidationException(
                ErrorCodes.PATIENT_INVALID_DNI,
                nameof(ErrorCodes.PATIENT_INVALID_DNI));
        }

        var password = request.Dni.ToString(
            CultureInfo.InvariantCulture);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            user = CreateApplicationUser(request.Email);

            var createResult = await _userManager.CreateAsync(
                user,
                password);

            if (!createResult.Succeeded)
            {
                var exception = new ConflictException(
                    ErrorCodes.REGISTER_USER_CONFLICT,
                    nameof(ErrorCodes.REGISTER_USER_CONFLICT));

                exception.WithDetail(
                    createResult.Errors.Select(
                        error => (error.Code, error.Description)));

                throw exception;
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                Roles.Patient);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var exception = new ConflictException(
                    ErrorCodes.REGISTER_USER_CONFLICT,
                    nameof(ErrorCodes.REGISTER_USER_CONFLICT));

                exception.WithDetail(
                    roleResult.Errors.Select(
                        error => (error.Code, error.Description)));

                throw exception;
            }

            var newPatient = new Patient(user.Id, request.Dni);

            try
            {
                await _persistence.Add(newPatient);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            await _logService.LogAsync(
                "Auth",
                "PATIENT_AUTO_REGISTERED",
                $"Paciente auto-registrado: {request.Email}");
        }
        else
        {
            if (user.Deleted)
            {
                await _logService.LogAsync(
                    "Auth",
                    "LOGIN_PATIENT_FAILED",
                    $"Intento de login de usuario eliminado: {request.Email}",
                    LogLevel.Warning);

                throw new AuthenticationException();
            }

            var isPatient = await _userManager.IsInRoleAsync(
                user,
                Roles.Patient);

            if (!isPatient)
            {
                await _logService.LogAsync(
                    "Auth",
                    "LOGIN_PATIENT_FAILED",
                    $"Usuario sin rol de paciente: {request.Email}",
                    LogLevel.Warning);

                throw new AuthenticationException();
            }

            var existingPatient = await _persistence.First<Patient>(
                patient => patient.UserId == user.Id)
                ?? throw new AuthenticationException();

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _logService.LogAsync(
                        "Auth",
                        "LOGIN_PATIENT_LOCKED",
                        $"Intento de login de cuenta bloqueada: {request.Email}",
                        LogLevel.Warning);

                    throw new AuthenticationException();
                }

                if (existingPatient.Dni != request.Dni)
                {
                    await _userManager.AccessFailedAsync(user);

                    await _logService.LogAsync(
                        "Auth",
                        "LOGIN_PATIENT_FAILED",
                        $"DNI no coincide para {request.Email}",
                        LogLevel.Warning);

                    throw new AuthenticationException();
                }

                var passwordResult = await _userManager.AddPasswordAsync(
                    user,
                    password);

                if (!passwordResult.Succeeded)
                {
                    var exception = new ConflictException(
                        ErrorCodes.REGISTER_USER_CONFLICT,
                        nameof(ErrorCodes.REGISTER_USER_CONFLICT));

                    exception.WithDetail(
                        passwordResult.Errors.Select(
                            error => (error.Code, error.Description)));

                    throw exception;
                }

                await _userManager.ResetAccessFailedCountAsync(user);
            }
            else
            {
                var passwordIsValid = await _signInManager.CheckPassword(
                    user,
                    password);

                if (!passwordIsValid)
                {
                    await _logService.LogAsync(
                        "Auth",
                        "LOGIN_PATIENT_FAILED",
                        $"Contraseña incorrecta para {request.Email}",
                        LogLevel.Warning);

                    throw new AuthenticationException();
                }
            }
        }

        var token = _jwtService.GenerateToken(
            user.Id,
            user.UserName!,
            Roles.Patient);

        await _logService.LogAsync(
            "Auth",
            "LOGIN_PATIENT_SUCCESS",
            $"Login exitoso para {request.Email}");

        return new LoginPatientModel.Response(
            token,
            Roles.Patient.ToUpperInvariant());
    }

    private static ApplicationUser CreateApplicationUser(string email)
    {
        var now = DateTime.UtcNow;

        return new ApplicationUser
        {
            UserName = email,
            Email = email,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
