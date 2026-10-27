
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests
{
    public class AppointmentServiceDeleteTests
    {
        private readonly IPersistence _persistence;
        private readonly ILogService _logger;
        private readonly AppointmentService _service;

        public AppointmentServiceDeleteTests()
        {
            _persistence = Substitute.For<IPersistence>();
            _logger = Substitute.For<ILogService>();
            _service = new AppointmentService(_persistence, _logger);
        }

        [Fact]
        public async Task DeleteAppointment_CuandoElTurnoEstaReservado_LoCancelaYLiberaElSlot()
        {
            // Arrange
            const string userId = "user-123";

            var slot = new AvailabilitySlot(
                availabilityRuleId: Guid.NewGuid(),
                slotDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                startTime: new TimeOnly(9, 0),
                endTime: new TimeOnly(9, 30)
            );

            slot.Book();

            var patient = new Patient(
                userId: userId,
                dni: 12345678
            );

            var appointment = new Appointment(
                patient.Id,
                slot.Id,
                "Control del endpoint delete"
            );

            SetAvailabilitySlot(appointment, slot);
            SetPatient(appointment, patient);

            _persistence
                .GetById<Appointment>(
                    appointment.Id,
                    Arg.Any<string[]>())
                .Returns(appointment);

            // Act
            await _service.DeleteAppointment(appointment.Id, userId);

            // Assert
            Assert.Equal(Estado.CANCELLED, appointment.Estado);
            Assert.Equal(SlotStatus.AVAILABLE, slot.Status);

            await _persistence.Received(1).BeginTransactionAsync();
            await _persistence.Received(1).Update(appointment);
            await _persistence.Received(1).Update(slot);
            await _persistence.Received(1).CommitTransactionAsync();

            await _persistence.DidNotReceive().RollbackTransactionAsync();
        }

        [Fact]
        public async Task DeleteAppointment_CuandoElTurnoNoExiste_LanzaEntityNotFoundException()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();

            _persistence
                .GetById<Appointment>(
                    idInexistente,
                    Arg.Any<string[]>())
                .Returns((Appointment?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.DeleteAppointment(idInexistente, "user-123"));

            await _persistence.DidNotReceive().BeginTransactionAsync();
            await _persistence.DidNotReceive().Update(
                Arg.Any<Appointment>());
            await _persistence.DidNotReceive().CommitTransactionAsync();
        }


        [Fact]
        public async Task DeleteAppointment_CuandoElTurnoPerteneceAOtroPaciente_LanzaAuthorizationException()
        {
            // Arrange
            const string propietarioId = "user-propietario";
            const string usuarioAjenoId = "user-ajeno";

            var slot = new AvailabilitySlot(
                availabilityRuleId: Guid.NewGuid(),
                slotDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                startTime: new TimeOnly(9, 0),
                endTime: new TimeOnly(9, 30)
            );

            slot.Book();

            var patient = new Patient(
                userId: propietarioId,
                dni: 12345678
            );

            var appointment = new Appointment(
                patient.Id,
                slot.Id,
                "Turno de otro paciente"
            );

            SetAvailabilitySlot(appointment, slot);
            SetPatient(appointment, patient);

            _persistence
                .GetById<Appointment>(
                    appointment.Id,
                    Arg.Any<string[]>())
                .Returns(appointment);

            // Act & Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => _service.DeleteAppointment(appointment.Id, usuarioAjenoId));

            await _persistence.DidNotReceive().BeginTransactionAsync();
            await _persistence.DidNotReceive().Update(
                Arg.Any<Appointment>());
            await _persistence.DidNotReceive().CommitTransactionAsync();
        }


        [Fact]
        public async Task GetAppointmentByDni_CuandoElPacienteNoExiste_LanzaEntityNotFoundException()
        {
            const string userId = "user-inexistente";

            _persistence
                .First<Patient>(Arg.Any<System.Linq.Expressions.Expression<Func<Patient, bool>>>())
                .Returns((Patient?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.GetAppointmentByDni(userId));

            await _persistence.DidNotReceive()
                .GetFiltered<Appointment>(Arg.Any<System.Linq.Expressions.Expression<Func<Appointment, bool>>>());
        }


        private static void SetAvailabilitySlot(
            Appointment appointment,
            AvailabilitySlot slot)
        {
            typeof(Appointment)
                .GetProperty(nameof(Appointment.AvailabilitySlot))!
                .SetValue(appointment, slot);
        }

        private static void SetPatient(
            Appointment appointment,
            Patient patient)
        {
            typeof(Appointment)
                .GetProperty(nameof(Appointment.Patient))!
                .SetValue(appointment, patient);
        }


    }
}
