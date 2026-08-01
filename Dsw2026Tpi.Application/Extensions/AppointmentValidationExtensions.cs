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

            if (slot.SlotDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new BusinessRuleException(
                    ErrorCodes.APPOINTMENT_PAST_DATE,
                    nameof(ErrorCodes.APPOINTMENT_PAST_DATE))
                    .WithDetail("AvailabilityId", "No se puede reservar un turno con fecha pasada");
            }
        }
    }
}
