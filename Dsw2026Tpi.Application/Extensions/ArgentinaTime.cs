namespace Dsw2026Tpi.Application.Extensions
{
    public static class ArgentinaTime
    {
        private static readonly TimeZoneInfo TimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Tucuman");

        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone);
    }
}