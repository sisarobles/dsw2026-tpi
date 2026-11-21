using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class DoctorServiceUpdateTests
{
    private readonly IPersistence _persistence;
    private readonly DoctorService _service;

    public DoctorServiceUpdateTests()
    {
        _persistence = Substitute.For<IPersistence>();
        _service = new DoctorService(_persistence);
    }

    [Fact]
    public async Task UpdateAsync_CuandoElMedicoEstaInactivo_LanzaEntityNotFoundException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        var doctor = new Doctor(
            "Dr. Test",
            "MP-123",
            Guid.NewGuid(),
            doctorId);

        doctor.Deactivate();

        _persistence.GetById<Doctor>(doctorId)
            .Returns(doctor);

        var request = new DoctorModel.Request(
            "Dr. Actualizado",
            "MP-456",
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.UpdateAsync(doctorId, request));

        await _persistence.DidNotReceive()
            .Update(Arg.Any<Doctor>());

        await _persistence.DidNotReceive()
            .GetById<Speciality>(Arg.Any<Guid>());
    }

    [Fact]
    public async Task UpdateAsync_CuandoElMedicoEstaActivo_ActualizaSusDatos()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var specialityId = Guid.NewGuid();

        var doctor = new Doctor(
            "Dr. Original",
            "MP-123",
            specialityId,
            doctorId);

        var speciality = new Speciality(
            "Cardiología",
            "Especialidad del corazón",
            specialityId);

        _persistence.GetById<Doctor>(doctorId)
            .Returns(doctor);

        _persistence.GetById<Speciality>(specialityId)
            .Returns(speciality);

        _persistence.Update(Arg.Any<Doctor>())
            .Returns(Task.FromResult(doctor));

        var request = new DoctorModel.Request(
            "Dr. Actualizado",
            "MP-456",
            specialityId);

        // Act
        var result = await _service.UpdateAsync(doctorId, request);

        // Assert
        Assert.Equal("Dr. Actualizado", result.Name);
        Assert.Equal("MP-456", result.LicenseNumber);
        Assert.True(result.IsActive);

        await _persistence.Received(1)
            .Update(doctor);
    }
}
