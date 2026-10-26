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
    public void Cancel_CuandoLaCitaEstaReservada_CambiaEstadoACancelled()
    {
        // Arrange
        var appointment = new Appointment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Consulta médica general");

        // Act
        appointment.Cancel();

        // Assert
        Assert.Equal(Estado.CANCELLED, appointment.Estado);
    }

    [Fact]
    public void Cancel_CuandoLaCitaYaEstaCancelada_LanzaBusinessRuleException()
    {
        // Arrange
        var appointment = new Appointment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Consulta médica general");

        appointment.Cancel();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => appointment.Cancel());

        Assert.Equal(Estado.CANCELLED, appointment.Estado);
    }

}