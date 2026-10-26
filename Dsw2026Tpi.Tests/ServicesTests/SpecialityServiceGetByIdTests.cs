using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class SpecialityServiceGetByIdTests
{
    private readonly IPersistence _persistence;
    private readonly SpecialityService _service;

    public SpecialityServiceGetByIdTests()
    {
        _persistence = Substitute.For<IPersistence>();
        _service = new SpecialityService(_persistence);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoLaEspecialidadExiste_DevuelveSusDatos()
    {
        // Arrange
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");

        _persistence.GetById<Speciality>(speciality.Id)
            .Returns(speciality);

        // Act
        var resultado = await _service.GetByIdAsync(speciality.Id);

        // Assert
        Assert.Equal(speciality.Id, resultado.Id);
        Assert.Equal("Cardiología", resultado.Name);
        Assert.True(resultado.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoLaEspecialidadNoExiste_LanzaExcepcion()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _persistence.GetById<Speciality>(idInexistente)
            .Returns((Speciality?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.GetByIdAsync(idInexistente));
    }

    [Fact]
    public async Task GetByIdAsync_CuandoLaEspecialidadEstaInactiva_LanzaExcepcion()
    {
        // Arrange
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");

        speciality.Deactivate();

        _persistence.GetById<Speciality>(speciality.Id)
            .Returns(speciality);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.GetByIdAsync(speciality.Id));
    }
}