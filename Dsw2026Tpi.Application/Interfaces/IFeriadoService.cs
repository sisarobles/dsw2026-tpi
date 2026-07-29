using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
        public interface IFeriadoService
        {
            // Indica si la fecha dada es un feriado o día no laborable según el archivo de configuración (feriados.json)
            bool EsFeriado(DateOnly fecha);

            // Devuelve el detalle del feriado (nombre y tipo) si la fecha lo es, o null si no.
            FeriadoInfo? GetFeriado(DateOnly fecha);
        }

        public record FeriadoInfo(DateOnly Fecha, string Nombre, string Tipo);

}
