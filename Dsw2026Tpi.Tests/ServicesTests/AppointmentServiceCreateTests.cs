
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests
{
    public class AppointmentServiceCreateTests
    {
        private readonly IPersistence _persistence;
        private readonly ILogService _logger;
        private readonly AppointmentService _service;

        public AppointmentServiceCreateTests()
        {
            _persistence = Substitute.For<IPersistence>();
            _logger = Substitute.For<ILogService>();
            _service = new AppointmentService(_persistence, _logger);
        }

        [Fact]
        public async Task CreateAppointment_CuandoEsValido_ConfirmaLaTransaccion()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            const long dni = 30123456;

            var rule = new AvailabilityRule(
                doctorId,
                DateTime.UtcNow.AddDays(1).Month,
                DateTime.UtcNow.AddDays(1).Year,
                DateTime.UtcNow.AddDays(1).DayOfWeek,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0));

            var slot = new AvailabilitySlot(
                rule.Id,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));

            SetAvailabilityRule(slot, rule);

            var patient = new Patient("test-user", dni, "Paciente de prueba");

            var request = new AppointmentModel.Request(
                doctorId,
                slot.Id,
                new AppointmentModel.PatientRequest(dni),
                "Control médico");

            _persistence
                .GetById<AvailabilitySlot>(
                    slot.Id,
                    Arg.Any<string[]>())
                .Returns(slot);

            _persistence
                .First<Patient>(Arg.Any<System.Linq.Expressions.Expression<Func<Patient, bool>>>())
                .Returns(patient);

            _persistence
                .Add(Arg.Any<Appointment>())
                .Returns(call => call.Arg<Appointment>());

            // Act
            await _service.CreateAppointment("user-123", request);

            // Assert
            Assert.Equal(SlotStatus.BOOKED, slot.Status);
            await _persistence.Received(1).BeginTransactionAsync();
            await _persistence.Received(1).Update(slot);
            await _persistence.Received(1).Add(Arg.Any<Appointment>());
            await _persistence.Received(1).CommitTransactionAsync();
            await _persistence.DidNotReceive().RollbackTransactionAsync();
        }

        [Fact]
        public async Task CreateAppointment_CuandoAgregarCitaFalla_RevierteLaTransaccion()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            const long dni = 30123456;
            var tomorrow = DateTime.UtcNow.AddDays(1);

            var rule = new AvailabilityRule(
                doctorId,
                tomorrow.Month,
                tomorrow.Year,
                tomorrow.DayOfWeek,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0));

            var slot = new AvailabilitySlot(
                rule.Id,
                DateOnly.FromDateTime(tomorrow),
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));

            SetAvailabilityRule(slot, rule);

            var patient = new Patient("test-user", dni, "Paciente de prueba");

            var request = new AppointmentModel.Request(
                doctorId,
                slot.Id,
                new AppointmentModel.PatientRequest(dni),
                "Control médico");

            _persistence
                .GetById<AvailabilitySlot>(
                    slot.Id,
                    Arg.Any<string[]>())
                .Returns(slot);

            _persistence
                .First<Patient>(Arg.Any<System.Linq.Expressions.Expression<Func<Patient, bool>>>())
                .Returns(patient);

            _persistence
                .Add(Arg.Any<Appointment>())
                .Returns<Task<Appointment>>(
                    _ => throw new InvalidOperationException("Error de prueba"));

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAppointment("user-123", request));

            // Assert
            await _persistence.Received(1).BeginTransactionAsync();
            await _persistence.Received(1).RollbackTransactionAsync();
            await _persistence.DidNotReceive().CommitTransactionAsync();
        }

        private static void SetAvailabilityRule(
            AvailabilitySlot slot,
            AvailabilityRule rule)
        {
            typeof(AvailabilitySlot)
                .GetProperty(nameof(AvailabilitySlot.AvailabilityRule))!
                .SetValue(slot, rule);
        }
    }
}
