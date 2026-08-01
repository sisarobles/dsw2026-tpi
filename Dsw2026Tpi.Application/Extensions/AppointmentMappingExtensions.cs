using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class AppointmentMappingExtensions
    {
        public static AppointmentModel.Response ToResponse(this Appointment appointment)
        {
            return appointment.ToResponse(appointment.Patient!, appointment.AvailabilitySlot!);
        }
        public static AppointmentModel.Response ToResponse(this Appointment appointment, Patient patient, AvailabilitySlot slot)
        {
            return new AppointmentModel.Response(
                appointment.Id,
                appointment.Estado.ToString(),
                appointment.Reason,
                new AppointmentModel.PatientSummary(
                    patient.Id, 
                    patient.Dni, 
                    patient.FullName ?? string.Empty
                ),
                new AppointmentModel.AvailabilitySlotSummary(
                    slot.Id, 
                    slot.Status.ToString(), 
                    slot.SlotDate, 
                    slot.StartTime, 
                    slot.EndTime
                )
            );
        }

        public static AppointmentSummaryModel.Response ToSummaryResponse(this Appointment appointment)
        {
            return new AppointmentSummaryModel.Response(
                appointment.Id,
                appointment.Estado.ToString(),
                appointment.Reason
            );
        }

        public static AppointmentSearchModel.Response ToSearchResponse(this Appointment appointment)
        {
            return new AppointmentSearchModel.Response(
                appointment.AvailabilitySlot?.AvailabilityRule?.Doctor.Speciality?.Name ?? string.Empty,
                appointment.AvailabilitySlot?.AvailabilityRule?.Doctor.Name ?? string.Empty,
                appointment.AvailabilitySlot?.StartTime ?? default
            );
        }
    }
}
