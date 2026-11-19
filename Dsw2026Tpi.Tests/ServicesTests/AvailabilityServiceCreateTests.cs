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
    public class AvailabilityServiceCreateTests
    {
        private readonly IPersistence _persistence = Substitute.For<IPersistence>();
        private readonly IFeriadoService _feriadoService = Substitute.For<IFeriadoService>();
        private readonly ILogService _logger = Substitute.For<ILogService>();

        private AvailabilityService CreateService() =>
            new AvailabilityService(_persistence, _feriadoService, _logger);

        [Fact]
        public async Task CreateAvailability_CuandoDoctorNoExiste_LanzaEntityNotFoundException()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            var request = new AvailabilityModel.Request(
                doctorId,
                new List<AvailabilityModel.DayRequest>
                {
                    new("Lunes", new TimeOnly(9, 0), new TimeOnly(12, 0))
                });

            _persistence.GetById<Doctor>(doctorId)
                .Returns((Doctor)null!);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => service.CreateAvailability(request));

            await _persistence.DidNotReceiveWithAnyArgs()
                .Add(Arg.Any<AvailabilityRule>());

            await _persistence.DidNotReceiveWithAnyArgs()
                .Add(Arg.Any<AvailabilitySlot>());
        }


        [Fact]
        public async Task CreateAvailability_CuandoDoctorEstaInactivo_LanzaEntityNotFoundException()
            {
                // Arrange
                var doctorId = Guid.NewGuid();

                var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);
                doctor.Deactivate();

                var request = new AvailabilityModel.Request(
                    doctorId,
                    new List<AvailabilityModel.DayRequest>
                    {
                        new("Lunes", new TimeOnly(9, 0), new TimeOnly(12, 0))
                    });

                _persistence.GetById<Doctor>(doctorId)
                    .Returns(doctor);

                var service = CreateService();

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(
                    () => service.CreateAvailability(request));

                await _persistence.DidNotReceiveWithAnyArgs()
                    .Add(Arg.Any<AvailabilityRule>());

                await _persistence.DidNotReceiveWithAnyArgs()
                    .Add(Arg.Any<AvailabilitySlot>());
            }

        [Fact]
        public async Task CreateAvailability_CuandoHorariosSeSuperponen_LanzaBusinessRuleException()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var now = DateTime.UtcNow;

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
                    new("Lunes", new TimeOnly(11, 0), new TimeOnly(14, 0))
                });

            _persistence.GetById<Doctor>(doctorId)
                .Returns(doctor);

            _persistence.GetFiltered<AvailabilityRule>(
                Arg.Any<System.Linq.Expressions.Expression<
                    Func<AvailabilityRule, bool>>>())
                .Returns(new List<AvailabilityRule> { reglaExistente });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => service.CreateAvailability(request));

            await _persistence.DidNotReceiveWithAnyArgs()
                .Add(Arg.Any<AvailabilityRule>());

            await _persistence.DidNotReceiveWithAnyArgs()
                .Add(Arg.Any<AvailabilitySlot>());
        }


[Fact]
public async Task CreateAvailability_CuandoRangoHorarioEsInvalido_LanzaValidationException()
{
    // Arrange
    var doctorId = Guid.NewGuid();

    var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

    var request = new AvailabilityModel.Request(
        doctorId,
        new List<AvailabilityModel.DayRequest>
        {
            new("Lunes", new TimeOnly(15, 0), new TimeOnly(9, 0))
        });

    _persistence.GetById<Doctor>(doctorId)
        .Returns(doctor);

    var service = CreateService();

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(
        () => service.CreateAvailability(request));

    await _persistence.DidNotReceiveWithAnyArgs()
        .Add(Arg.Any<AvailabilityRule>());

    await _persistence.DidNotReceiveWithAnyArgs()
        .Add(Arg.Any<AvailabilitySlot>());
}


[Fact]
public async Task CreateAvailability_CuandoHorarioEsValido_GeneraSlotsDe30Minutos()
{
    // Arrange
    var doctorId = Guid.NewGuid();
    var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var day = today.DayOfWeek;

    var dayNames = new Dictionary<DayOfWeek, string>
    {
        { DayOfWeek.Monday, "Lunes" },
        { DayOfWeek.Tuesday, "Martes" },
        { DayOfWeek.Wednesday, "Miércoles" },
        { DayOfWeek.Thursday, "Jueves" },
        { DayOfWeek.Friday, "Viernes" },
        { DayOfWeek.Saturday, "Sábado" },
        { DayOfWeek.Sunday, "Domingo" }
    };

    var request = new AvailabilityModel.Request(
        doctorId,
        new List<AvailabilityModel.DayRequest>
        {
            new(dayNames[day], new TimeOnly(9, 0), new TimeOnly(11, 0))
        });

    _persistence.GetById<Doctor>(doctorId)
        .Returns(doctor);

    _persistence.GetFiltered<AvailabilityRule>(
        Arg.Any<System.Linq.Expressions.Expression<
            Func<AvailabilityRule, bool>>>())
        .Returns(new List<AvailabilityRule>());

    _feriadoService.EsFeriado(Arg.Any<DateOnly>())
        .Returns(false);

    var slotsGuardados = new List<AvailabilitySlot>();

    _persistence.Add(Arg.Any<AvailabilitySlot>())
    .Returns(call =>
    {
        var slot = call.Arg<AvailabilitySlot>()
            ?? throw new InvalidOperationException("El slot no puede ser nulo.");

        slotsGuardados.Add(slot);
        return slot;
    });

    var service = CreateService();

    // Act
    var response = await service.CreateAvailability(request);

    // Assert
    Assert.Single(response);
    Assert.NotEmpty(slotsGuardados);

    Assert.All(slotsGuardados, slot =>
    {
        Assert.Equal(
            TimeSpan.FromMinutes(30),
            slot.EndTime.ToTimeSpan() - slot.StartTime.ToTimeSpan());

        Assert.True(slot.StartTime >= new TimeOnly(9, 0));
        Assert.True(slot.EndTime <= new TimeOnly(11, 0));
    });
}


[Fact]
public async Task CreateAvailability_CuandoDiaEsFeriado_NoGeneraSlots()
{
    // Arrange
    var doctorId = Guid.NewGuid();
    var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var dayNames = new Dictionary<DayOfWeek, string>
    {
        { DayOfWeek.Monday, "Lunes" },
        { DayOfWeek.Tuesday, "Martes" },
        { DayOfWeek.Wednesday, "Miércoles" },
        { DayOfWeek.Thursday, "Jueves" },
        { DayOfWeek.Friday, "Viernes" },
        { DayOfWeek.Saturday, "Sábado" },
        { DayOfWeek.Sunday, "Domingo" }
    };

    var request = new AvailabilityModel.Request(
        doctorId,
        new List<AvailabilityModel.DayRequest>
        {
            new(dayNames[today.DayOfWeek],
                new TimeOnly(9, 0),
                new TimeOnly(12, 0))
        });

    _persistence.GetById<Doctor>(doctorId)
        .Returns(doctor);

    _persistence.GetFiltered<AvailabilityRule>(
        Arg.Any<System.Linq.Expressions.Expression<
            Func<AvailabilityRule, bool>>>())
        .Returns(new List<AvailabilityRule>());

    // Simulamos que todas las fechas son feriados.
    _feriadoService.EsFeriado(Arg.Any<DateOnly>())
        .Returns(true);

    var service = CreateService();

    // Act
    var response = await service.CreateAvailability(request);

    // Assert
    Assert.Empty(response);

    await _persistence.DidNotReceiveWithAnyArgs()
        .Add(Arg.Any<AvailabilityRule>());

    await _persistence.DidNotReceiveWithAnyArgs()
        .Add(Arg.Any<AvailabilitySlot>());
}


[Fact]
public async Task CreateAvailability_CuandoHorarioNoEsMultiploDe30_NoGeneraSlotsFueraDelRango()
{
    // Arrange
    var doctorId = Guid.NewGuid();
    var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var dayNames = new Dictionary<DayOfWeek, string>
    {
        { DayOfWeek.Monday, "Lunes" },
        { DayOfWeek.Tuesday, "Martes" },
        { DayOfWeek.Wednesday, "Miércoles" },
        { DayOfWeek.Thursday, "Jueves" },
        { DayOfWeek.Friday, "Viernes" },
        { DayOfWeek.Saturday, "Sábado" },
        { DayOfWeek.Sunday, "Domingo" }
    };

    var request = new AvailabilityModel.Request(
        doctorId,
        new List<AvailabilityModel.DayRequest>
        {
            new(dayNames[today.DayOfWeek],
                new TimeOnly(9, 0),
                new TimeOnly(10, 15))
        });

    _persistence.GetById<Doctor>(doctorId)
        .Returns(doctor);

    _persistence.GetFiltered<AvailabilityRule>(
        Arg.Any<System.Linq.Expressions.Expression<
            Func<AvailabilityRule, bool>>>())
        .Returns(new List<AvailabilityRule>());

    _feriadoService.EsFeriado(Arg.Any<DateOnly>())
        .Returns(false);

    var slotsGuardados = new List<AvailabilitySlot>();

    _persistence.Add(Arg.Any<AvailabilitySlot>())
        .Returns(call =>
        {
            var slot = call.Arg<AvailabilitySlot>()
                ?? throw new InvalidOperationException("Slot nulo.");

            slotsGuardados.Add(slot);
            return slot;
        });

    var service = CreateService();

    // Act
    await service.CreateAvailability(request);

    // Assert
    Assert.NotEmpty(slotsGuardados);

    Assert.All(slotsGuardados, slot =>
    {
        Assert.True(
            slot.EndTime <= new TimeOnly(10, 15),
            $"El slot {slot.StartTime}-{slot.EndTime} excede el horario permitido.");
    });
}

    }
}
