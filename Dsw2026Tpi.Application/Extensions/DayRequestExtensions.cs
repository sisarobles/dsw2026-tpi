using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class DayRequestExtensions
    {
        public static IEnumerable<AvailabilitySlot> GenerateSlots(
            this AvailabilityModel.DayRequest dayRequest,
            Guid ruleId,
            DateOnly today,
            int daysInMonth,
            int month,
            int year, IFeriadoService feriadoService)
        {
            var slots = new List<AvailabilitySlot>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);

                if (date.DayOfWeek == dayRequest.Day && date >= today && !feriadoService.EsFeriado(date))
                {
                    var current = dayRequest.StartTime;

                    while (current < dayRequest.EndTime)
                    {
                        var slotEnd = current.AddMinutes(30);
                        slots.Add(new AvailabilitySlot(ruleId, date, current, slotEnd));
                        current = slotEnd;
                    }
                }
            }

            return slots;
        }
    }
}
