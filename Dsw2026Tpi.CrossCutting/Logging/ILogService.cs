namespace Dsw2026Tpi.CrossCutting.Logging
{
    public interface ILogService
    {
        Task RegistrarAsync(
            string modulo,
            string accion,
            string detalle,
            LogNivel nivel = LogNivel.Info);

    }
    public enum LogNivel
    {
        Info,
        Warning,
        Error 
    }
}
