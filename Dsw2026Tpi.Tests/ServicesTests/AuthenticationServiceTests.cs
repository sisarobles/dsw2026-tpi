using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class AuthenticationServiceTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInService;
    private readonly IPersistence _persistence;
    private readonly ILogService _logService;
    private readonly JwtService _jwtService;

    public AuthenticationServiceTests()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();

        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            store,
            Options.Create(new IdentityOptions()),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<ApplicationUser>>>());

        _signInService = Substitute.For<ISignInService>();
        _persistence = Substitute.For<IPersistence>();
        _logService = Substitute.For<ILogService>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "EstaEsUnaClaveDePruebaDeAlMenos32Bytes",
                ["Jwt:Issuer"] = "Dsw2026Tpi.Tests",
                ["Jwt:Audience"] = "Dsw2026Tpi.Tests",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        _jwtService = new JwtService(configuration);
    }

    private AuthenticationService CreateService()
    {
        return new AuthenticationService(
            _userManager,
            _signInService,
            _jwtService,
            _persistence,
            _logService);
    }

    private static ApplicationUser CreateUser(
        string email = "usuario@test.com",
        bool deleted = false,
        string? passwordHash = null)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            Deleted = deleted,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task LoginAdmin_CuandoCredencialesYRolSonValidos_RetornaToken()
    {
        var user = CreateUser("admin@test.com");

        var request = new LoginAdminModel.Request(
            "admin@test.com",
            "Password123");

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _signInService.CheckPassword(user, request.Password)
            .Returns(true);

        _userManager.IsInRoleAsync(user, Roles.Administrator)
            .Returns(true);

        var service = CreateService();

        var response = await service.LoginAdmin(request);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(Roles.Administrator.ToUpperInvariant(), response.Role);
    }

    [Fact]
    public async Task LoginAdmin_CuandoLaContrasenaEsIncorrecta_LanzaAuthenticationException()
    {
        var user = CreateUser("admin@test.com");

        var request = new LoginAdminModel.Request(
            "admin@test.com",
            "Password123");

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _signInService.CheckPassword(user, request.Password)
            .Returns(false);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginAdmin(request));
    }

    [Fact]
    public async Task LoginAdmin_CuandoElUsuarioNoEsAdministrador_LanzaAuthenticationException()
    {
        var user = CreateUser("paciente@test.com");

        var request = new LoginAdminModel.Request(
            "paciente@test.com",
            "Password123");

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _signInService.CheckPassword(user, request.Password)
            .Returns(true);

        _userManager.IsInRoleAsync(user, Roles.Administrator)
            .Returns(false);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginAdmin(request));
    }

    [Fact]
    public async Task LoginAdmin_CuandoElUsuarioEstaEliminado_LanzaAuthenticationException()
    {
        var user = CreateUser("admin@test.com", deleted: true);

        var request = new LoginAdminModel.Request(
            "admin@test.com",
            "Password123");

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _signInService.CheckPassword(user, request.Password)
            .Returns(true);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginAdmin(request));

        await _userManager.DidNotReceive()
            .IsInRoleAsync(user, Roles.Administrator);
    }

    [Fact]
    public async Task LoginPatient_CuandoElPacienteExisteYLaContrasenaEsValida_RetornaToken()
    {
        var user = CreateUser(
            "paciente@test.com",
            passwordHash: "hash-de-prueba");

        var patient = new Patient(user.Id, 12345678);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(true);

        _persistence.First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(patient);

        _signInService.CheckPassword(user, "12345678")
            .Returns(true);

        var service = CreateService();

        var response = await service.LoginPatient(request);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(Roles.Patient.ToUpperInvariant(), response.Role);

        await _signInService.Received(1)
            .CheckPassword(user, "12345678");
    }

    [Fact]
    public async Task LoginPatient_CuandoLaContrasenaEsIncorrecta_LanzaAuthenticationException()
    {
        var user = CreateUser(
            "paciente@test.com",
            passwordHash: "hash-de-prueba");

        var patient = new Patient(user.Id, 12345678);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            87654321);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(true);

        _persistence.First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(patient);

        _signInService.CheckPassword(user, "87654321")
            .Returns(false);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginPatient(request));
    }

    [Fact]
    public async Task LoginPatient_CuandoElUsuarioNoTieneRolPaciente_LanzaAuthenticationException()
    {
        var user = CreateUser("usuario@test.com");

        var request = new LoginPatientModel.Request(
            "usuario@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(false);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginPatient(request));

        await _persistence.DidNotReceiveWithAnyArgs()
            .First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>());
    }

    [Fact]
    public async Task LoginPatient_CuandoElUsuarioEstaEliminado_LanzaAuthenticationException()
    {
        var user = CreateUser(
            "paciente@test.com",
            deleted: true);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginPatient(request));

        await _userManager.DidNotReceive()
            .IsInRoleAsync(user, Roles.Patient);
    }

    [Fact]
    public async Task LoginPatient_CuandoElDniNoCoincideEnCuentaAntigua_LanzaAuthenticationExceptionYRegistraIntentoFallido()
    {
        var user = CreateUser("paciente@test.com");

        var patient = new Patient(user.Id, 12345678);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            87654321);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(true);

        _persistence.First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(patient);

        _userManager.IsLockedOutAsync(user)
            .Returns(false);

        _userManager.AccessFailedAsync(user)
            .Returns(IdentityResult.Success);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginPatient(request));

        await _userManager.Received(1)
            .AccessFailedAsync(user);

        await _userManager.DidNotReceive()
            .AddPasswordAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task LoginPatient_CuandoLaCuentaAntiguaEstaBloqueada_LanzaAuthenticationException()
    {
        var user = CreateUser("paciente@test.com");

        var patient = new Patient(user.Id, 12345678);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(true);

        _persistence.First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(patient);

        _userManager.IsLockedOutAsync(user)
            .Returns(true);

        var service = CreateService();

        await Assert.ThrowsAsync<AuthenticationException>(
            () => service.LoginPatient(request));

        await _userManager.Received(1)
            .IsLockedOutAsync(user);

        await _userManager.DidNotReceive()
            .AccessFailedAsync(user);

        await _userManager.DidNotReceive()
            .AddPasswordAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task LoginPatient_CuandoLaCuentaAntiguaNoTienePassword_LaMigra()
    {
        var user = CreateUser("paciente@test.com");

        var patient = new Patient(user.Id, 12345678);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(true);

        _persistence.First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(patient);

        _userManager.IsLockedOutAsync(user)
            .Returns(false);

        _userManager.AddPasswordAsync(user, "12345678")
            .Returns(IdentityResult.Success);

        _userManager.ResetAccessFailedCountAsync(user)
            .Returns(IdentityResult.Success);

        var service = CreateService();

        var response = await service.LoginPatient(request);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(Roles.Patient.ToUpperInvariant(), response.Role);

        await _userManager.Received(1)
            .AddPasswordAsync(user, "12345678");

        await _userManager.Received(1)
            .ResetAccessFailedCountAsync(user);

        await _signInService.DidNotReceiveWithAnyArgs()
            .CheckPassword(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task LoginPatient_CuandoFallaLaMigracionDePassword_LanzaConflictException()
    {
        var user = CreateUser("paciente@test.com");

        var patient = new Patient(user.Id, 12345678);

        var request = new LoginPatientModel.Request(
            "paciente@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns(user);

        _userManager.IsInRoleAsync(user, Roles.Patient)
            .Returns(true);

        _persistence.First<Patient>(
                Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(patient);

        _userManager.IsLockedOutAsync(user)
            .Returns(false);

        _userManager.AddPasswordAsync(user, "12345678")
            .Returns(IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordError",
                    Description = "No se pudo establecer la contraseña."
                }));

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(
            () => service.LoginPatient(request));
    }

    [Fact]
    public async Task LoginPatient_CuandoEsUnUsuarioNuevo_CreaUsuarioYPacienteConPassword()
    {
        var request = new LoginPatientModel.Request(
            "nuevo@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns((ApplicationUser?)null);

        _userManager.CreateAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>())
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(
                Arg.Any<ApplicationUser>(),
                Roles.Patient)
            .Returns(IdentityResult.Success);

        var service = CreateService();

        var response = await service.LoginPatient(request);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(Roles.Patient.ToUpperInvariant(), response.Role);

        await _userManager.Received(1)
            .CreateAsync(
                Arg.Is<ApplicationUser>(
                    user => user != null && user.Email == request.Email),
                "12345678");

        await _userManager.Received(1)
            .AddToRoleAsync(
                Arg.Is<ApplicationUser>(
                    user => user != null && user.Email == request.Email),
                Roles.Patient);

        await _persistence.Received(1)
            .Add(Arg.Is<Patient>(
                patient => patient != null && patient.Dni == request.Dni));
    }

    [Fact]
    public async Task LoginPatient_CuandoElDniTieneSieteDigitos_CreaUsuarioYPaciente()
    {
        var request = new LoginPatientModel.Request(
            "nuevo@test.com",
            1234567);

        _userManager.FindByEmailAsync(request.Email)
            .Returns((ApplicationUser?)null);

        _userManager.CreateAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>())
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(
                Arg.Any<ApplicationUser>(),
                Roles.Patient)
            .Returns(IdentityResult.Success);

        var service = CreateService();

        var response = await service.LoginPatient(request);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));

        await _userManager.Received(1)
            .CreateAsync(
                Arg.Is<ApplicationUser>(
                    user => user != null && user.Email == request.Email),
                "1234567");

        await _persistence.Received(1)
            .Add(Arg.Is<Patient>(
                patient => patient != null && patient.Dni == 1234567));
    }

    [Fact]
    public async Task LoginPatient_CuandoFallaLaAsignacionDelRol_EliminaElUsuarioYLanzaConflictException()
    {
        var request = new LoginPatientModel.Request(
            "nuevo@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns((ApplicationUser?)null);

        _userManager.CreateAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>())
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(
                Arg.Any<ApplicationUser>(),
                Roles.Patient)
            .Returns(IdentityResult.Failed(
                new IdentityError
                {
                    Code = "RoleAssignmentFailed",
                    Description = "No se pudo asignar el rol."
                }));

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(
            () => service.LoginPatient(request));

        await _userManager.Received(1)
            .DeleteAsync(Arg.Any<ApplicationUser>());

        await _persistence.DidNotReceiveWithAnyArgs()
            .Add(Arg.Any<Patient>());
    }

    [Fact]
    public async Task LoginPatient_CuandoFallaLaCreacionDelUsuario_LanzaConflictException()
    {
        var request = new LoginPatientModel.Request(
            "nuevo@test.com",
            12345678);

        _userManager.FindByEmailAsync(request.Email)
            .Returns((ApplicationUser?)null);

        _userManager.CreateAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>())
            .Returns(IdentityResult.Failed(
                new IdentityError
                {
                    Code = "DuplicateUserName",
                    Description = "El usuario ya existe."
                }));

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(
            () => service.LoginPatient(request));

        await _userManager.DidNotReceiveWithAnyArgs()
            .AddToRoleAsync(
                Arg.Any<ApplicationUser>(),
                Arg.Any<string>());

        await _persistence.DidNotReceiveWithAnyArgs()
            .Add(Arg.Any<Patient>());
    }
}
