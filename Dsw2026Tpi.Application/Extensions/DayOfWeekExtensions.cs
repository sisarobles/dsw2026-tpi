using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class DayOfWeekExtensions
    {
        public static string ToSpanish(this DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            DayOfWeek.Sunday => "Domingo",
            _ => day.ToString()
        };
    }
}
