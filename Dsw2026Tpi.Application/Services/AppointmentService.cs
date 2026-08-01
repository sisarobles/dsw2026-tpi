using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Logging;
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
        private readonly ILogService _logger;


        public AppointmentService(IPersistence persistence, ILogService logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<AppointmentModel.Response> CreateAppointment(AppointmentModel.Request request)
        {
            request.Reason.ValidateReason();

            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId, nameof(AvailabilitySlot.AvailabilityRule))
                       ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            slot.ValidateSlotForBooking(request.DoctorId);

            var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni)
                ?? throw new EntityNotFoundException(nameof(Patient));
            var patientId = patient.Id; 

            try
            {
                slot.Book();
                await _persistence.Update(slot);

                var appointment = new Appointment(patientId, slot.Id, request.Reason);
                await _persistence.Add(appointment);
                await _logger.RegistrarAsync(
                        modulo: "Appointments",
                        accion: "CreateAppointment",
                        detalle: $"Cita {appointment.Id} registrada para el paciente {patientId}, slot {slot.Id}");

                return appointment.ToResponse(patient, slot);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _logger.RegistrarAsync(
                        modulo: "Appointments",
                        accion: "CreateAppointment",
                        detalle: $"Conflicto de concurrencia al reservar el slot {slot.Id}",
                        nivel: LogNivel.Warning); 
                throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT, nameof(ErrorCodes.APPOINTMENT_CONFLICT));
            }
        }

        public async Task DeleteAppointment(Guid idAppointment)
        {
            var appointment = await _persistence.GetById<Appointment>(idAppointment, nameof(Appointment.AvailabilitySlot)) 
                ?? throw new EntityNotFoundException(nameof(Appointment));
            appointment.Cancel();
            appointment.AvailabilitySlot!.Release();
            await _persistence.Update<Appointment>(appointment);
            await _persistence.Update<AvailabilitySlot>(appointment.AvailabilitySlot);

            await _logger.RegistrarAsync(
                        modulo: "Appointments",
                        accion: "DeleteAppointment",
                        detalle: $"Cita {appointment.Id} del paciente {appointment.PatientId} eliminada");

        }

        public async Task<IEnumerable<AppointmentSummaryModel.Response>> GetAppointmentByDni(long dni) 
        {
            var patient = await _persistence.First<Patient>(p => p.Dni == dni)
                ?? throw new EntityNotFoundException(nameof(Patient));
            var appointments = await _persistence.GetFiltered<Appointment>(a =>
                (a.PatientId == patient.Id) &&
                (a.Estado == Estado.BOOKED));
            return appointments.Select(a => a.ToSummaryResponse());
        }

        public async Task<Pagination<AppointmentSearchModel.Response>> GetAppointmentBySearch(AppointmentSearchModel.Request request, int pageSize, int pageIndex)
        {
            var appointments = await _persistence.Paginate<Appointment, DateOnly>(pageSize, pageIndex,
                request.ToSearchPredicate(),
                a => a.AvailabilitySlot!.SlotDate,
                AppointmentQueryExtensions.SearchIncludes
            );

            await _logger.RegistrarAsync(
                modulo: "Appointments",
                accion: "GetAppointmentBySearch",
                detalle: $"Búsqueda de citas realizada. Filtros: SpecialtyId={request.SpecialtyId}, DoctorId={request.DoctorId}, PatientDni={request.PatientDni}, Date={request.Date}. Página {pageIndex}, tamaño {pageSize}"
            );

            return appointments.Map(a => a.ToSearchResponse());
        }
    }
}
