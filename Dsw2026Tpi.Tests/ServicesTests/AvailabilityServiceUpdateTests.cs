using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests
{
    public class AvailabilityServiceUpdateTests
    {
        private readonly IPersistence _persistence = Substitute.For<IPersistence>();
        private readonly IFeriadoService _feriadoService = Substitute.For<IFeriadoService>();
        private readonly ILogService _logger = Substitute.For<ILogService>();

        private AvailabilityService CreateService() =>
            new AvailabilityService(_persistence, _feriadoService, _logger);

        [Fact]
        public async Task UpdateAvailability_CuandoNoHaySlotsReservados_SobreescribeElMes()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var now = DateTime.Now;

            var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

            var reglaExistente = new AvailabilityRule(
                doctorId,
                now.Month,
                now.Year,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0));

            var slotExistente = new AvailabilitySlot(
                reglaExistente.Id,
                DateOnly.FromDateTime(now).AddDays(7),
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));

            var request = new AvailabilityModel.Request(
                doctorId,
                new List<AvailabilityModel.DayRequest>
                {
                    new("Miércoles", new TimeOnly(14, 0), new TimeOnly(16, 0))
                });

            _persistence.GetById<Doctor>(doctorId).Returns(doctor);

            _persistence.GetFiltered<AvailabilityRule>(
                    Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(
                    new List<AvailabilityRule> { reglaExistente },
                    new List<AvailabilityRule>());

            _persistence.GetFiltered<AvailabilitySlot>(
                    Arg.Any<Expression<Func<AvailabilitySlot, bool>>>())
                .Returns(new List<AvailabilitySlot> { slotExistente });

            _feriadoService.EsFeriado(Arg.Any<DateOnly>()).Returns(false);

            _persistence.Add(Arg.Any<AvailabilityRule>())
                .Returns<AvailabilityRule>(
                    c => c.Arg<AvailabilityRule>()
                        ?? throw new InvalidOperationException(
                            "No se recibió una regla de disponibilidad."));

            _persistence.Add(Arg.Any<AvailabilitySlot>())
                .Returns<AvailabilitySlot>(
                    c => c.Arg<AvailabilitySlot>()
                        ?? throw new InvalidOperationException(
                            "No se recibió un turno."));

            // Act
            var service = CreateService();
            var response = await service.UpdateAvailability(request);

            // Assert: soft delete de la regla y sus slots.
            Assert.True(reglaExistente.Deleted);
            Assert.True(slotExistente.Deleted);

            await _persistence.Received(1).Update(reglaExistente);
            await _persistence.Received(1).Update(slotExistente);

            // No debe haber eliminación física.
            await _persistence.DidNotReceiveWithAnyArgs()
                .Delete(Arg.Any<AvailabilityRule>());

            await _persistence.DidNotReceiveWithAnyArgs()
                .Delete(Arg.Any<AvailabilitySlot>());

            // Se crean las nuevas disponibilidades.
            await _persistence.Received(1).Add(Arg.Any<AvailabilityRule>());
            await _persistence.ReceivedWithAnyArgs()
                .Add(Arg.Any<AvailabilitySlot>());

            Assert.NotEmpty(response);
            Assert.Single(response);

            // Verificar transacción exitosa.
            await _persistence.Received(1).BeginTransactionAsync();
            await _persistence.Received(1).CommitTransactionAsync();
            await _persistence.DidNotReceive().RollbackTransactionAsync();
        }

        [Fact]
        public async Task UpdateAvailability_CuandoHaySlotsReservados_LanzaExcepcion()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var now = DateTime.Now;

            var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

            var reglaExistente = new AvailabilityRule(
                doctorId,
                now.Month,
                now.Year,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0));

            var request = new AvailabilityModel.Request(
                doctorId,
                new List<AvailabilityModel.DayRequest>
                {
                    new("Lunes", new TimeOnly(14, 0), new TimeOnly(16, 0))
                });

            _persistence.GetById<Doctor>(doctorId).Returns(doctor);

            _persistence.GetFiltered<AvailabilityRule>(
                    Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(new List<AvailabilityRule> { reglaExistente });

            var slotReservado = new AvailabilitySlot(
                reglaExistente.Id,
                DateOnly.FromDateTime(now),
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));

            slotReservado.Book();

            _persistence.GetFiltered<AvailabilitySlot>(
                    Arg.Any<Expression<Func<AvailabilitySlot, bool>>>())
                .Returns(new List<AvailabilitySlot> { slotReservado });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => service.UpdateAvailability(request));

            // La regla y el slot reservado deben permanecer intactos.
            Assert.False(reglaExistente.Deleted);
            Assert.False(slotReservado.Deleted);

            await _persistence.DidNotReceiveWithAnyArgs()
                .Update(Arg.Any<AvailabilityRule>());

            await _persistence.DidNotReceiveWithAnyArgs()
                .Update(Arg.Any<AvailabilitySlot>());

            await _persistence.DidNotReceiveWithAnyArgs()
                .Delete(Arg.Any<AvailabilityRule>());

            await _persistence.DidNotReceiveWithAnyArgs()
                .Add(Arg.Any<AvailabilityRule>());

            // Verificar rollback.
            await _persistence.Received(1).BeginTransactionAsync();
            await _persistence.DidNotReceive().CommitTransactionAsync();
            await _persistence.Received(1).RollbackTransactionAsync();
        }

        [Fact]
        public async Task UpdateAvailability_CuandoFallaLaCreacionDeUnaRegla_RevierteLaTransaccion()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var now = DateTime.Now;

            var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

            var reglaExistente = new AvailabilityRule(
                doctorId,
                now.Month,
                now.Year,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0));

            var request = new AvailabilityModel.Request(
                doctorId,
                new List<AvailabilityModel.DayRequest>
                {
                    new("Miércoles", new TimeOnly(14, 0), new TimeOnly(16, 0))
                });

            _persistence.GetById<Doctor>(doctorId).Returns(doctor);

            _persistence.GetFiltered<AvailabilityRule>(
                    Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(
                    new List<AvailabilityRule> { reglaExistente },
                    new List<AvailabilityRule>());

            _persistence.GetFiltered<AvailabilitySlot>(
                    Arg.Any<Expression<Func<AvailabilitySlot, bool>>>())
                .Returns(new List<AvailabilitySlot>());

            _feriadoService.EsFeriado(Arg.Any<DateOnly>()).Returns(false);

            _persistence.Add(Arg.Any<AvailabilityRule>())
                .Returns<AvailabilityRule>(
                    _ => throw new InvalidOperationException(
                        "Error simulado al guardar la nueva regla."));

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAvailability(request));

            // La actualización debe revertirse.
            await _persistence.Received(1).BeginTransactionAsync();
            await _persistence.DidNotReceive().CommitTransactionAsync();
            await _persistence.Received(1).RollbackTransactionAsync();
        }
    }
}
