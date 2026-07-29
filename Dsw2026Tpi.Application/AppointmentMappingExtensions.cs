using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application
{
    public static class AppointmentMappingExtensions
    {
        public static AppointmentModel.Response ToResponse(this Appointment appointment)
        {
            return new AppointmentModel.Response(
                appointment.Id,
                appointment.Estado.ToString()
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
