using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/auth")]
public class AuthenticationController : AppController
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenRevocationService _tokenRevocationService;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ITokenRevocationService tokenRevocationService)
    {
        _authenticationService = authenticationService;
        _tokenRevocationService = tokenRevocationService;
    }

    [AllowAnonymous]
    [HttpPost("admin/login")]
    [EnableRateLimiting("AdminAuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginAdminModel.Request request)
    {
        var result = await _authenticationService.LoginAdmin(request);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("patient/login")]
    [EnableRateLimiting("PatientAuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginPatient(
        [FromBody] LoginPatientModel.Request request)
    {
        var result = await _authenticationService.LoginPatient(request);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var expiration = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (string.IsNullOrWhiteSpace(jti) ||
            string.IsNullOrWhiteSpace(expiration) ||
            !long.TryParse(
                expiration,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var expirationTimestamp))
        {
            return Unauthorized();
        }

        DateTime expiresAt;

        try
        {
            expiresAt = DateTimeOffset
                .FromUnixTimeSeconds(expirationTimestamp)
                .UtcDateTime;
        }
        catch (ArgumentOutOfRangeException)
        {
            return Unauthorized();
        }

        await _tokenRevocationService.RevokeAsync(jti, expiresAt);

        return Ok(new
        {
            message = "Sesión cerrada correctamente."
        });
    }
}
