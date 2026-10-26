using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class DoctorServiceGetByIdTests
{
    private readonly IPersistence _persistence;
    private readonly DoctorService _service;

    public DoctorServiceGetByIdTests()
    {
        _persistence = Substitute.For<IPersistence>();
        _service = new DoctorService(_persistence);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoElMedicoExiste_DevuelveSusDatos()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

        _persistence.GetById<Doctor>(doctorId)
            .Returns(doctor);

        // Act
        var resultado = await _service.GetByIdAsync(doctorId);

        // Assert
        Assert.Equal(doctorId, resultado.Id);
        Assert.Equal("Dr. Test", resultado.Name);
        Assert.True(resultado.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoElMedicoNoExiste_LanzaExcepcion()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        _persistence.GetById<Doctor>(doctorId)
            .Returns((Doctor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.GetByIdAsync(doctorId));
    }

    [Fact]
    public async Task GetByIdAsync_CuandoElMedicoEstaInactivo_LanzaExcepcion()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var doctor = new Doctor("Dr. Test", "MP-123", null, doctorId);

        doctor.Deactivate();

        _persistence.GetById<Doctor>(doctorId)
            .Returns(doctor);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.GetByIdAsync(doctorId));
    }

}