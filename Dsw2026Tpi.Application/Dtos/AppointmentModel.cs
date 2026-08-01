using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {
        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientRequest? Patient, string Reason);
        public record PatientRequest(long Dni);
        public record Response(Guid Id, string Status, string Reason, PatientSummary Patient, AvailabilitySlotSummary AvailabilitySlot);
        public record PatientSummary(Guid Id, long Dni, string FullName);
        public record AvailabilitySlotSummary(Guid Id, string Status, DateOnly SlotDate, TimeOnly StartTime, TimeOnly EndTime);
    }
    public record AppointmentSummaryModel
    {
        public record Response(Guid AppointmentId,string Status, string Reason);
    }

    public record AppointmentSearchModel
    {
        public record Request(Guid? SpecialtyId, Guid? DoctorId, long? PatientDni, DateOnly? Date);
        public record Response(string Specialty, string DoctorName, TimeOnly AvailableTime);
    }

}
