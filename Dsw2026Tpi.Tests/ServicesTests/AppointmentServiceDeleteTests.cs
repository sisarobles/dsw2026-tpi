using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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
            var slot = new AvailabilitySlot(
                availabilityRuleId: Guid.NewGuid(),
                slotDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                startTime: new TimeOnly(9, 0),
                endTime: new TimeOnly(9, 30));
            
            slot.Book(); 

            var appointment = new Appointment(Guid.NewGuid(), slot.Id, "Control del endpoint delete");
            SetAvailabilitySlot(appointment, slot); //ver método static abajo

            _persistence.GetById<Appointment>(appointment.Id, Arg.Any<string[]>())
                .Returns(appointment);

            //Act
            await _service.DeleteAppointment(appointment.Id);

            //Assert
            Assert.Equal(Estado.CANCELLED, appointment.Estado);
            Assert.Equal(SlotStatus.AVAILABLE, slot.Status);
            await _persistence.Received(1).Update(appointment);
            await _persistence.Received(1).Update(slot);
        }
        [Fact]
        public async Task DeleteAppointment_CuandoElTurnoNoExiste_LanzaEntityNotFoundException()
        {
            //Arrange
            var idInexistente = Guid.NewGuid();

           _persistence.GetById<Appointment>(idInexistente, Arg.Any<string[]>())
           .Returns((Appointment?)null);

            //Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.DeleteAppointment(idInexistente));

            await _persistence.DidNotReceive().Update(Arg.Any<Appointment>());
        }

        //appointment.AvailabilitySlot tiene set privado, así que en el test lo seteamos aquí para simular 
        private static void SetAvailabilitySlot(Appointment appointment, AvailabilitySlot slot)
        {
            typeof(Appointment)
                .GetProperty(nameof(Appointment.AvailabilitySlot))!
                .SetValue(appointment, slot);
        }
    }
}
