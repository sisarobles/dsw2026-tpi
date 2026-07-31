using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        public record Request(Guid DoctorId, List<DayRequest> Days);
        public record DayRequest(DayOfWeek Day, TimeOnly StartTime, TimeOnly EndTime);
        public record Response(Guid id, string Day, TimeOnly StartTime, TimeOnly EndTime);

        public record SlotResponse(Guid Id, DateOnly SlotDate, TimeOnly StartTime, TimeOnly EndTime);
    }
}
