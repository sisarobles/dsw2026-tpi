using Microsoft.Extensions.Logging;
using AppLogLevel = Dsw2026Tpi.CrossCutting.Logging.LogLevel;

namespace Dsw2026Tpi.CrossCutting.Logging;

public class LogService : ILogService
{
    private readonly ILogger<LogService> _logger;

    public LogService(ILogger<LogService> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(
        string module,
        string action,
        string detail,
        AppLogLevel level = AppLogLevel.Information)
    {
        var message = $"[{module}] {action} - {detail}";

        switch (level)
        {
            case AppLogLevel.Warning:
                _logger.LogWarning(message);
                break;
            case AppLogLevel.Error:
                _logger.LogError(message);
                break;
            default:
                _logger.LogInformation(message);
                break;
        }

        return Task.CompletedTask;
    }
}
