
using System.Text.Json;
using Dsw2026Tpi.Api.Middlewares;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ApiTests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CuandoHayConflicto_Devuelve409ConCodigoDeError()
    {
        // Arrange
        RequestDelegate next = _ =>
            throw new ConflictException(
                ErrorCodes.APPOINTMENT_CONFLICT,
                nameof(ErrorCodes.APPOINTMENT_CONFLICT));

        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        context.Response.Body.Position = 0;

        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        using var json = JsonDocument.Parse(body);

        Assert.Equal(
            nameof(ErrorCodes.APPOINTMENT_CONFLICT),
            json.RootElement.GetProperty("errorCode").GetString());
    }
}
