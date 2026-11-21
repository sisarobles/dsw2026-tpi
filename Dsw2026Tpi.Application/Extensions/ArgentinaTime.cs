namespace Dsw2026Tpi.Application.Extensions;

public static class ArgentinaTime
{
    private static readonly TimeZoneInfo TimeZone = ResolveTimeZone();

    public static DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone);

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "America/Argentina/Tucuman");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Argentina Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                throw new InvalidOperationException(
                    "No se encontró una zona horaria válida para Argentina.");
            }
        }
        catch (InvalidTimeZoneException)
        {
            throw new InvalidOperationException(
                "La configuración de la zona horaria de Argentina es inválida.");
        }
    }
}
