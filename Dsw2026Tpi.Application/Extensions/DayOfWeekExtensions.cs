using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.CrossCutting.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class DayOfWeekExtensions
    {
        public static DayOfWeek FromString(string day) => day.ToLower() switch
        {
            "monday" or "lunes" => DayOfWeek.Monday,
            "tuesday" or "martes" => DayOfWeek.Tuesday,
            "wednesday" or "miércoles" or "miercoles" => DayOfWeek.Wednesday,
            "thursday" or "jueves" => DayOfWeek.Thursday,
            "friday" or "viernes" => DayOfWeek.Friday,
            "saturday" or "sábado" or "sabado" => DayOfWeek.Saturday,
            "sunday" or "domingo" => DayOfWeek.Sunday,
            _ => throw new ValidationException(
                ErrorCodes.VALIDATION_ERROR,
                nameof(ErrorCodes.VALIDATION_ERROR))
        };

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
