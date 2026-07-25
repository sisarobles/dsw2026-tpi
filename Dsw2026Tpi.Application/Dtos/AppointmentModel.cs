using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {
        public record Request(Guid DoctorId, Guid AvailabilityId, PatientRequest? Patient, string Reason);
        public record PatientRequest(long Dni);
        public record Response(Guid Id, string Status);

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
