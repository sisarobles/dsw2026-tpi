using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class AvailabilityValidationExtensions
    {
        public static void ValidateTimeRange(this AvailabilityModel.DayRequest dayRequest)
        {
            if (dayRequest.StartTime >= dayRequest.EndTime)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR,nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("StartTime", "must_be_less_than_end_time").WithDetail("EndTime", "must_be_greater_than_start_time");
        }

        public static async Task ValidateNoOverlap(
            this AvailabilityModel.DayRequest dayRequest,
            IPersistence persistence,
            Guid doctorId,
            int month,
            int year)
        {
            var dayOfWeek = DayOfWeekExtensions.FromString(dayRequest.Day);

            var rulesOfTheDay = await persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == doctorId &&
                     r.Month == month &&
                     r.Year == year &&
                     r.DayOfWeek == dayOfWeek &&
                     !r.Deleted);

            var haySolapamiento = rulesOfTheDay.Any(r =>
                dayRequest.StartTime < r.EndTime &&
                dayRequest.EndTime > r.StartTime);

            if (haySolapamiento)
                throw new BusinessRuleException(ErrorCodes.AVAILABILITY_OVERLAP, nameof(ErrorCodes.AVAILABILITY_OVERLAP))
                    .WithDetail("days", "schedule_overlap_detected");
        }
    
    }
}
