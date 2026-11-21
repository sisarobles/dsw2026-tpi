using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;
        private readonly ILogService _logger;

        public AppointmentService(IPersistence persistence, ILogService logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<AppointmentModel.Response> CreateAppointment(
            string userId,
            AppointmentModel.Request request)
        {
            request.Reason.ValidateReason();
            request.Patient.ValidatePatient();

            var slot = await _persistence.GetById<AvailabilitySlot>(
                request.AvailabilitySlotId,
                nameof(AvailabilitySlot.AvailabilityRule))
                ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            slot.ValidateSlotForBooking(request.DoctorId);

            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);

            if (doctor == null || !doctor.IsActive)
            {
                throw new EntityNotFoundException(nameof(Doctor));
            }

            var patient = await _persistence.First<Patient>(
                p => p.UserId == userId)
                ?? throw new EntityNotFoundException(nameof(Patient));

            if (patient.Dni != request.Patient!.Dni)
            {
                throw new AuthorizationException();
            }

            var patientId = patient.Id;

            await _persistence.BeginTransactionAsync();

            Appointment appointment;

            try
            {
                var slotBooked = await _persistence.TryBookAvailabilitySlot(slot.Id);

                if (!slotBooked)
                {
                    throw new ConflictException(
                        ErrorCodes.APPOINTMENT_CONFLICT,
                        nameof(ErrorCodes.APPOINTMENT_CONFLICT));
                }

                slot.Book();

                appointment = new Appointment(patientId, slot.Id, request.Reason);

                await _persistence.Add(appointment);

                await _persistence.CommitTransactionAsync();
            }
            catch (ConflictException)
            {
                await _persistence.RollbackTransactionAsync();

                await _logger.LogAsync(
                    module: "Appointments",
                    action: "CreateAppointment",
                    detail: $"Conflicto al reservar el slot {slot.Id}",
                    level: LogLevel.Warning);

                throw;
            }
            catch (DbUpdateConcurrencyException)
            {
                await _persistence.RollbackTransactionAsync();

                await _logger.LogAsync(
                    module: "Appointments",
                    action: "CreateAppointment",
                    detail: $"Conflicto de concurrencia al reservar el slot {slot.Id}",
                    level: LogLevel.Warning);

                throw new ConflictException(
                    ErrorCodes.APPOINTMENT_CONFLICT,
                    nameof(ErrorCodes.APPOINTMENT_CONFLICT));
            }
            catch
            {
                await _persistence.RollbackTransactionAsync();
                throw;
            }

            await _logger.LogAsync(
                module: "Appointments",
                action: "CreateAppointment",
                detail: $"Cita {appointment.Id} registrada para el paciente {patientId}, slot {slot.Id}");

            return appointment.ToResponse(patient, slot);
        }

        public async Task DeleteAppointment(Guid idAppointment, string userId)
        {
            var appointment = await _persistence.GetById<Appointment>(
                idAppointment,
                nameof(Appointment.AvailabilitySlot),
                nameof(Appointment.Patient))
                ?? throw new EntityNotFoundException(nameof(Appointment));

            if (appointment.Patient?.UserId != userId)
            {
                throw new AuthorizationException();
            }

            await _persistence.BeginTransactionAsync();

            try
            {
                var cancelled = await _persistence.TryCancelAppointment(
                    appointment.Id,
                    appointment.AvailabilitySlotId);

                if (!cancelled)
                {
                    throw new ConflictException(
                        ErrorCodes.CANCEL_APPOINTMENT,
                        nameof(ErrorCodes.CANCEL_APPOINTMENT));
                }

                await _persistence.CommitTransactionAsync();
            }
            catch
            {
                await _persistence.RollbackTransactionAsync();
                throw;
            }

            await _logger.LogAsync(
                module: "Appointments",
                action: "DeleteAppointment",
                detail: $"Cita {appointment.Id} del paciente {appointment.PatientId} cancelada");
        }

        public async Task<IEnumerable<AppointmentSummaryModel.Response>>
            GetAppointmentByDni(string userId)
        {
            var patient = await _persistence.First<Patient>(
                p => p.UserId == userId)
                ?? throw new EntityNotFoundException(nameof(Patient));

            var appointments = await _persistence.GetFiltered<Appointment>(
                a => a.PatientId == patient.Id &&
                     a.Estado == Estado.BOOKED,
                AppointmentQueryExtensions.SearchIncludes);

            return appointments!.Select(a => a.ToSummaryResponse());
        }

        public async Task<Pagination<AppointmentSearchModel.Response>>
            GetAppointmentBySearch(
                AppointmentSearchModel.Request request,
                int pageSize,
                int pageIndex)
        {
            var appointments = await _persistence.Paginate<Appointment, DateOnly>(
                pageSize,
                pageIndex,
                request.ToSearchPredicate(),
                a => a.AvailabilitySlot!.SlotDate,
                AppointmentQueryExtensions.SearchIncludes
            );

            await _logger.LogAsync(
                module: "Appointments",
                action: "GetAppointmentBySearch",
                detail: $"Búsqueda de citas realizada. Filtros: SpecialtyId={request.SpecialtyId}, DoctorId={request.DoctorId}, PatientDni={request.PatientDni}, Date={request.Date}. Página {pageIndex}, tamaño {pageSize}"
            );

            return appointments.Map(a => a.ToSearchResponse());
        }
    }
}
