using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Xunit;

namespace Dsw2026Tpi.Tests.DomainTests;

public class AppointmentTests
{
    [Fact]
    public void Create_ConDatosValidos_CreaCitaReservada()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var reason = "Consulta médica general";

        // Act
        var appointment = new Appointment(
            patientId,
            slotId,
            reason);

        // Assert
        Assert.Equal(patientId, appointment.PatientId);
        Assert.Equal(slotId, appointment.AvailabilitySlotId);
        Assert.Equal(reason, appointment.Reason);
        Assert.Equal(Estado.BOOKED, appointment.Estado);
    }

    [Fact]
    public void Cancel_CuandoLaCitaEstaReservada_CancelaCitaYLiberaTurno()
    {
        // Arrange
        var slotId = Guid.NewGuid();

        var appointment = new Appointment(
            Guid.NewGuid(),
            slotId,
            "Consulta médica general");

        var slot = CreateBookedSlot(slotId);

        // Act
        appointment.Cancel(slot);

        // Assert
        Assert.Equal(Estado.CANCELLED, appointment.Estado);
        Assert.Equal(SlotStatus.AVAILABLE, slot.Status);
    }

    [Fact]
    public void Cancel_CuandoLaCitaYaEstaCancelada_LanzaBusinessRuleException()
    {
        // Arrange
        var slotId = Guid.NewGuid();

        var appointment = new Appointment(
            Guid.NewGuid(),
            slotId,
            "Consulta médica general");

        var slot = CreateBookedSlot(slotId);

        appointment.Cancel(slot);

        // Act & Assert
        Assert.Throws<BusinessRuleException>(
            () => appointment.Cancel(slot));

        Assert.Equal(Estado.CANCELLED, appointment.Estado);
        Assert.Equal(SlotStatus.AVAILABLE, slot.Status);
    }

    [Fact]
    public void Cancel_CuandoElTurnoNoCorrespondeALaCita_LanzaInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Consulta médica general");

        var slot = CreateBookedSlot(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => appointment.Cancel(slot));

        Assert.Equal(Estado.BOOKED, appointment.Estado);
        Assert.Equal(SlotStatus.BOOKED, slot.Status);
    }

    private static AvailabilitySlot CreateBookedSlot(Guid slotId)
    {
        var slot = new AvailabilitySlot(
            Guid.NewGuid(),
            new DateOnly(2026, 1, 1),
            new TimeOnly(9, 0),
            new TimeOnly(10, 0),
            slotId);

        slot.Book();

        return slot;
    }
}
