using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System.Linq.Expressions;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class AppointmentQueryExtensions
    {
        public static Expression<Func<Appointment, bool>> ToSearchPredicate(this AppointmentSearchModel.Request request)
        {
            return a => (request.SpecialtyId == null || a.AvailabilitySlot!.AvailabilityRule!.Doctor!.SpecialityId == request.SpecialtyId)
                     && (request.DoctorId == null || a.AvailabilitySlot!.AvailabilityRule!.DoctorId == request.DoctorId)
                     && (request.Date == null || a.AvailabilitySlot!.SlotDate == request.Date)
                     && (request.PatientDni == null || a.Patient!.Dni == request.PatientDni);
        }

        public static readonly string[] SearchIncludes =
        [
            nameof(Appointment.AvailabilitySlot) + "." +
            nameof(AvailabilitySlot.AvailabilityRule) + "." +
            nameof(AvailabilityRule.Doctor) + "." +
            nameof(Doctor.Speciality),
            nameof(Appointment.Patient)
         ];
    }
}
