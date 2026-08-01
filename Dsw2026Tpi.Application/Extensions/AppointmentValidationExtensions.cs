using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class AppointmentValidationExtensions
    {
        public static void ValidateReason(this string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 5)
            {
                throw new ValidationException(
                    ErrorCodes.VALIDATION_ERROR,
                    nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("Reason", "El motivo debe tener al menos 5 caracteres");
            }
        }

        public static void ValidateSlotForBooking(this AvailabilitySlot slot, Guid expectedDoctorId)
        {
            if (slot.AvailabilityRule?.DoctorId != expectedDoctorId)
            {
                throw new ValidationException(
                    ErrorCodes.VALIDATION_ERROR,
                    nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("DoctorId", "El turno seleccionado no pertenece al doctor indicado");
            }

            if (slot.Status != SlotStatus.AVAILABLE)
            {
                throw new ConflictException(
                    ErrorCodes.APPOINTMENT_CONFLICT,
                    nameof(ErrorCodes.APPOINTMENT_CONFLICT))
                    .WithDetail("AvailabilityId", "El turno ya no está disponible");
            }

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var isPastDate = slot.SlotDate < today;
            var isPastTimeToday = slot.SlotDate == today && slot.StartTime < TimeOnly.FromDateTime(now);

            if (isPastDate || isPastTimeToday)
            {
                throw new BusinessRuleException(ErrorCodes.APPOINTMENT_PAST_DATE, nameof(ErrorCodes.APPOINTMENT_PAST_DATE))
                    .WithDetail("AvailabilityId", "No se puede reservar un turno con fecha u horario pasado");
            }
        }
        public static void ValidatePatient(this AppointmentModel.PatientRequest? patient)
        {
            if (patient == null)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("Patient", "Ingresar el paciente es obligatorio");

            var dniLength = patient.Dni.ToString().Length;
            if (patient.Dni <= 0 || dniLength < 7 || dniLength > 10)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("Dni", "El DNI debe tener entre 7 y 10 dígitos");
        }
    }
}
