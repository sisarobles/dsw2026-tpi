using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Interfaces
{
    public interface IFeriadoService
    {
        bool EsFeriado(DateOnly fecha);
        FeriadoInfo? GetFeriado(DateOnly fecha);
    }

    public record FeriadoInfo(DateOnly Fecha, string Nombre, string Tipo);

}
