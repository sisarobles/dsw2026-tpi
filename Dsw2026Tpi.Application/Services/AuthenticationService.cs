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
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPersistence _persistence;
    private readonly ILogService _logService;

    public AuthenticationService(UserManager<ApplicationUser> userManager,
        ISignInService signInManager,
        RoleManager<IdentityRole> roleManager,
        JwtService jwtService,
        ILogger<AuthenticationService> logger,
        IPersistence persistence,
        ILogService logService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _logger = logger;
        _persistence = persistence;
        _logService = logService;
    }

    public async Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request)
    {
        if (!request.Email.IsEmailValid()) 
        throw new AuthenticationException();
        if (request.Password.Length < 8)
            throw new ValidationException(
                ErrorCodes.REGISTER_USER_INVALID,
                nameof(ErrorCodes.REGISTER_USER_INVALID));
        var user = await _userManager.FindByEmailAsync(request.Email) 
            ?? throw new AuthenticationException();
        var result = await _signInManager.CheckPassword(user, request.Password);

        if (!result)
        {
            _logger.LogError("Intento de login fallido para: {Email}", request.Email);
            await _logService.RegistrarAsync("Auth", "LOGIN_ADMIN_FAILED", $"Intento de login fallido para {request.Email}", LogNivel.Warning);
            throw new AuthenticationException();
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        var token = _jwtService.GenerateToken(user.UserName!, role);

        await _logService.RegistrarAsync("Auth", "LOGIN_ADMIN_SUCCESS", $"Login exitoso para {request.Email}");

        return new LoginAdminModel.Response(
            token,
            role
       
        );
    }

    public async Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new AuthenticationException();

        if (request.Dni < 1_000_000 || request.Dni > 99_999_999)
            throw new ValidationException(ErrorCodes.PATIENT_INVALID_DNI, nameof(ErrorCodes.PATIENT_INVALID_DNI));

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
                throw new ConflictException(ErrorCodes.REGISTER_USER_CONFLICT, nameof(ErrorCodes.REGISTER_USER_CONFLICT))
                    .WithDetail(createResult.Errors.Select(e => (e.Code, e.Description)));

            _ = await _userManager.AddToRoleAsync(user, Roles.Patient);

            var newPatient = new Patient(user.Id, request.Dni);
            await _persistence.Add(newPatient);

            _logger.LogInformation("Paciente auto-registrado: {Email}", request.Email);
            
            await _logService.RegistrarAsync("Auth", "PATIENT_AUTO_REGISTERED", $"Paciente auto-registrado: {request.Email}");
        }
        else
        {
            var existingPatient = await _persistence.First<Patient>(p => p.UserId == user.Id)
        ?? throw new AuthenticationException();

            if (existingPatient.Dni != request.Dni)
            {
                await _logService.RegistrarAsync("Auth", "LOGIN_PATIENT_FAILED", $"DNI no coincide para {request.Email}", LogNivel.Warning);
                throw new AuthenticationException();
            }
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        var token = _jwtService.GenerateToken(user.UserName!, role);
        await _logService.RegistrarAsync("Auth", "LOGIN_PATIENT_SUCCESS", $"Login exitoso para {request.Email}");
        return new LoginPatientModel.Response(token, role);
    }

    public async Task<RegisterModel.Response> Register(RegisterModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new ValidationException(ErrorCodes.REGISTER_USER_INVALID,
            nameof(ErrorCodes.REGISTER_USER_INVALID));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded) throw new ConflictException(ErrorCodes.REGISTER_USER_CONFLICT, nameof(ErrorCodes.REGISTER_USER_CONFLICT))
                .WithDetail(result.Errors.Select(e => (e.Code, e.Description)));
       
        _ = await _userManager.AddToRoleAsync(user, Roles.Administrator);

        _logger.LogInformation("Usuario registrado: {Email}", request.Email);
        await _logService.RegistrarAsync("Auth", "ADMIN_REGISTERED", $"Usuario admin registrado: {request.Email}");

        return new RegisterModel.Response(request.Email);
    }
}
