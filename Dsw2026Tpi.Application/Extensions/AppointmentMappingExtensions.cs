using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class AppointmentMappingExtensions
    {
        public static AppointmentModel.Response ToResponse(
            this Appointment appointment)
        {
            return appointment.ToResponse(
                appointment.Patient!,
                appointment.AvailabilitySlot!);
        }

        public static AppointmentModel.Response ToResponse(
            this Appointment appointment,
            Patient patient,
            AvailabilitySlot slot)
        {
            return new AppointmentModel.Response(
                appointment.Id,
                appointment.Estado.ToString(),
                appointment.Reason,
                new AppointmentModel.PatientSummary(
                    patient.Dni,
                    patient.FullName ?? string.Empty),
                new AppointmentModel.AvailabilitySlotSummary(
                    slot.Id,
                    slot.Status.ToString(),
                    slot.SlotDate,
                    slot.StartTime,
                    slot.EndTime));
        }

        public static AppointmentSummaryModel.Response ToSummaryResponse(
            this Appointment appointment)
        {
            var slot = appointment.AvailabilitySlot!;
            var doctor = slot.AvailabilityRule!.Doctor!;
            var speciality = doctor.Speciality!;

            return new AppointmentSummaryModel.Response(
                appointment.Id,
                appointment.Estado.ToString(),
                appointment.Reason,
                new AppointmentSummaryModel.DoctorSummary(
                    doctor.Id,
                    doctor.Name,
                    new AppointmentSummaryModel.SpecialtySummary(
                        speciality.Id,
                        speciality.Name)),
                new AppointmentSummaryModel.AvailableTimeSummary(
                    slot.SlotDate,
                    slot.StartTime,
                    slot.EndTime));
        }

        public static AppointmentSearchModel.Response ToSearchResponse(
            this Appointment appointment)
        {
            var patient = appointment.Patient!;
            var slot = appointment.AvailabilitySlot!;
            var doctor = slot.AvailabilityRule!.Doctor!;
            var speciality = doctor.Speciality!;

            return new AppointmentSearchModel.Response(
                appointment.Id,
                appointment.Estado.ToString(),
                new AppointmentModel.PatientSummary(
                    patient.Dni,
                    patient.FullName ?? string.Empty),
                new AppointmentSearchModel.DoctorSummary(
                    doctor.Id,
                    doctor.Name,
                    new AppointmentSearchModel.SpecialtySummary(
                        speciality.Id,
                        speciality.Name)),
                new AppointmentSearchModel.AvailableTimeSummary(
                    slot.SlotDate,
                    slot.StartTime,
                    slot.EndTime));
        }
    }
}