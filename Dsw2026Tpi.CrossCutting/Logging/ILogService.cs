namespace Dsw2026Tpi.CrossCutting.Logging
{
    public interface ILogService
    {
        Task LogAsync(
            string module,
            string action,
            string detail,
            LogLevel level = LogLevel.Information);
    }

    public enum LogLevel
    {
        Information,
        Warning,
        Error
    }
}
