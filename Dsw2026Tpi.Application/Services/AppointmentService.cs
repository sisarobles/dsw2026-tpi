using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;

        public AppointmentService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<AppointmentModel.Response> CreateAppointment(AppointmentModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR));

            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId, nameof(AvailabilitySlot.AvailabilityRule))
                       ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            if (slot.AvailabilityRule.DoctorId != request.DoctorId)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR));

            if (slot.Status != SlotStatus.AVAILABLE)
                throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT, nameof(ErrorCodes.APPOINTMENT_CONFLICT));

            if (slot.SlotDate < DateOnly.FromDateTime(DateTime.Now)) //UtcNow
                throw new BusinessRuleException(ErrorCodes.APPOINTMENT_PAST_DATE, nameof(ErrorCodes.APPOINTMENT_PAST_DATE));

            //FALTA (pendiente: Patient): buscar/crear el paciente por DNI
            var patientId = Guid.Empty; //reemplazar por patient.Id cuando esté Patient

            try
            {
                slot.Book();
                await _persistence.Update(slot);

                var appointment = new Appointment(patientId, slot.Id, request.Reason);
                await _persistence.Add(appointment);

                return new AppointmentModel.Response(appointment.Id, appointment.Estado.ToString());
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT, nameof(ErrorCodes.APPOINTMENT_CONFLICT));
            }
        }

        public async Task DeleteAppointment(Guid idAppointment)
        {
            var appointment = await _persistence.GetById<Appointment>(idAppointment) ?? throw new EntityNotFoundException(nameof(Appointment));
            appointment.Cancel();
            await _persistence.Update<Appointment>(appointment);
        }

        public Task<IEnumerable<AppointmentSummaryModel.Response>> GetAppointmentByDni(long dni)
        {
            throw new NotImplementedException();
        }

        public async Task<Pagination<AppointmentSearchModel.Response>> GetAppointmentBySearch(AppointmentSearchModel.Request request, int pageSize, int pageIndex)
        {
            var slots = await _persistence.Paginate<AvailabilitySlot, DateOnly>(pageSize, pageIndex,
                s => (request.SpecialtyId == null || s.AvailabilityRule.Doctor.SpecialityId == request.SpecialtyId)
                  && (request.DoctorId == null || s.AvailabilityRule.DoctorId == request.DoctorId)
                  && (request.Date == null || s.SlotDate == request.Date),
                s => s.SlotDate,
                nameof(AvailabilitySlot.AvailabilityRule) + "." + nameof(AvailabilityRule.Doctor) + "." + nameof(Doctor.Speciality)
            );

            return slots.Map(s => new AppointmentSearchModel.Response(
                s.AvailabilityRule.Doctor.Speciality?.Name ?? string.Empty,
                s.AvailabilityRule.Doctor.Name,
                s.StartTime
            ));
        }
    }
}
