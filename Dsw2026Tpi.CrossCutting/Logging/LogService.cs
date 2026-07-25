using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.CrossCutting.Logging;

/// <summary>
/// Implementación de <see cref="ILogService"/> basada en el logging nativo de .NET.
/// </summary>
public class LogService : ILogService
{
    private readonly ILogger<LogService> _logger;

    public LogService(ILogger<LogService> logger)
    {
        _logger = logger;
    }

    public Task RegistrarAsync(
        string modulo,
        string accion,
        string detalle,
        LogNivel nivel = LogNivel.Info)
    {
        var mensaje = $"[{modulo}] {accion} - {detalle}";

        switch (nivel)
        {
            case LogNivel.Warning:
                _logger.LogWarning(mensaje);
                break;
            case LogNivel.Error:
                _logger.LogError(mensaje);
                break;
            default:
                _logger.LogInformation(mensaje);
                break;
        }

        return Task.CompletedTask;
    }
}