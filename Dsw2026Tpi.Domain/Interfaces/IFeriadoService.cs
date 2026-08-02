namespace Dsw2026Tpi.Domain.Interfaces
{
    public interface IFeriadoService
    {
        bool EsFeriado(DateOnly fecha);
        FeriadoInfo? GetFeriado(DateOnly fecha);
    }

    public record FeriadoInfo(DateOnly Fecha, string Nombre, string Tipo);

}
