using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class DoctorServiceLicenseNumberTests
{
    private readonly IPersistence _persistence;
    private readonly DoctorService _service;

    public DoctorServiceLicenseNumberTests()
    {
        _persistence = Substitute.For<IPersistence>();
        _service = new DoctorService(_persistence);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    public async Task CreateAsync_CuandoLaMatriculaEsInvalida_LanzaValidationException(
        string? licenseNumber)
    {
        // Arrange
        var specialtyId = Guid.NewGuid();

        var specialty = new Speciality(
            "Cardiología",
            "Especialidad del corazón",
            specialtyId);

        _persistence.GetById<Speciality>(specialtyId)
            .Returns(specialty);

        var request = new DoctorModel.Request(
            "Dr. Test",
            licenseNumber!,
            specialtyId);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(request));

        await _persistence.DidNotReceive()
            .Add(Arg.Any<Doctor>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    public async Task UpdateAsync_CuandoLaMatriculaEsInvalida_LanzaValidationException(
        string? licenseNumber)
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();

        var doctor = new Doctor(
            "Dr. Test",
            "MP-123",
            specialtyId,
            doctorId);

        var specialty = new Speciality(
            "Cardiología",
            "Especialidad del corazón",
            specialtyId);

        _persistence.GetById<Doctor>(doctorId)
            .Returns(doctor);

        _persistence.GetById<Speciality>(specialtyId)
            .Returns(specialty);

        var request = new DoctorModel.Request(
            "Dr. Test",
            licenseNumber!,
            specialtyId);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(doctorId, request));

        await _persistence.DidNotReceive()
            .Update(Arg.Any<Doctor>());
    }

    [Fact]
    public void ValidateLicenseNumber_CuandoTiene50Caracteres_NoLanzaExcepcion()
    {
        // Arrange
        var licenseNumber = new string('1', 50);

        // Act & Assert
        licenseNumber.ValidateLicenseNumber();
    }
}
