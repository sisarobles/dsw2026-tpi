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
            var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

            var reglaExistente = new AvailabilityRule(
                doctorId,
                DateTime.UtcNow.Month,
                DateTime.UtcNow.Year,
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

            _persistence.GetFiltered<AvailabilityRule>(Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(
                    new List<AvailabilityRule> { reglaExistente },
                    new List<AvailabilityRule>());

            _persistence.GetFiltered<AvailabilitySlot>(Arg.Any<Expression<Func<AvailabilitySlot, bool>>>())
                .Returns(new List<AvailabilitySlot>());

            _feriadoService.EsFeriado(Arg.Any<DateOnly>()).Returns(false);

            _persistence.Add(Arg.Any<AvailabilityRule>())
                .Returns(c => c.Arg<AvailabilityRule>());
            _persistence.Add(Arg.Any<AvailabilitySlot>())
                .Returns(c => c.Arg<AvailabilitySlot>());

            // Act
            var service = CreateService();
            var response = await service.UpdateAvailability(request);

            // Assert
            await _persistence.Received(1).Delete(reglaExistente);
            await _persistence.Received(1).Add(Arg.Any<AvailabilityRule>());
            await _persistence.ReceivedWithAnyArgs().Add(Arg.Any<AvailabilitySlot>());
            Assert.NotEmpty(response);
            Assert.Single(response);
        }
    

    [Fact]
        public async Task UpdateAvailability_CuandoHaySlotsReservados_LanzaExcepcion()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

            var reglaExistente = new AvailabilityRule(
                doctorId,
                DateTime.UtcNow.Month,
                DateTime.UtcNow.Year,
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

            
            _persistence.GetFiltered<AvailabilityRule>(Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(new List<AvailabilityRule> { reglaExistente });

            
            var slotReservado = new AvailabilitySlot(
                reglaExistente.Id,
                DateOnly.FromDateTime(DateTime.UtcNow),
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));
            slotReservado.Book(); 

            _persistence.GetFiltered<AvailabilitySlot>(Arg.Any<Expression<Func<AvailabilitySlot, bool>>>())
                .Returns(new List<AvailabilitySlot> { slotReservado });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateAvailability(request));
            await _persistence.DidNotReceiveWithAnyArgs().Delete(Arg.Any<AvailabilityRule>());
            await _persistence.DidNotReceiveWithAnyArgs().Add(Arg.Any<AvailabilityRule>());
        }
    }
}