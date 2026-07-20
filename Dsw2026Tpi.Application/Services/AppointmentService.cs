using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

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
            // 1. Buscar el slot y validar que exista
            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId)
                       ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            // 2. Validar que el slot pertenezca al doctor indicado (RN01, coherencia del request)
            if (slot.AvailabilityRule.DoctorId != request.DoctorId)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR));

            // 3. Validar que esté disponible (RN03)
            if (slot.Status != SlotStatus.AVAILABLE)
                throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT, nameof(ErrorCodes.APPOINTMENT_CONFLICT));

            // 4. Validar que no sea una fecha/hora pasada (RN04)
            if (slot.SlotDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BusinessRuleException(ErrorCodes.APPOINTMENT_PAST_DATE, nameof(ErrorCodes.APPOINTMENT_PAST_DATE));

            // TODO (pendiente Jaz - Patient): buscar/crear el paciente por DNI.
            // Por ahora, placeholder para que compile:
            // var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni.ToString())
            //               ?? throw new EntityNotFoundException(nameof(Patient));
            var patientId = Guid.Empty; // reemplazar por patient.Id cuando esté Patient

            // 5. Crear la cita
            var appointment = new Appointment(patientId, slot.Id, request.Reason);
            await _persistence.Add(appointment);

            // TODO (pendiente Meli - AvailabilitySlot): falta un método público en AvailabilitySlot
            // para pasar el Status de AVAILABLE a BOOKED (algo como slot.Book()), porque el setter
            // es privado y no hay ningún método que lo cambie todavía. Pedirle que lo agregue.
            // Una vez que exista:
            // slot.Book();
            // await _persistence.Update(slot);

            return new AppointmentModel.Response(appointment.Id, appointment.Estado.ToString());
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
