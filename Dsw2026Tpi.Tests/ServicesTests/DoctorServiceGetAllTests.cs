using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class DoctorServiceGetAllTests
{
    private readonly IPersistence _persistence;
    private readonly DoctorService _service;

    public DoctorServiceGetAllTests()
    {
        _persistence = Substitute.For<IPersistence>();
        _service = new DoctorService(_persistence);
    }

    [Fact]
    public async Task GetAll_CuandoFiltraPorEspecialidad_DevuelveMedicosDeEsaEspecialidad()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();

        Expression<Func<Doctor, bool>>? filtroCapturado = null;

        _persistence.Paginate<Doctor, string>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Do<Expression<Func<Doctor, bool>>>(f => filtroCapturado = f),
                Arg.Any<Expression<Func<Doctor, string>>>(),
                Arg.Any<string[]>())
            .Returns(Task.FromResult(
                new Pagination<Doctor>(10, 1, [], 0)));

        // Act
        await _service.GetAll(10, 1, specialtyId: specialtyId);

        // Assert
        Assert.NotNull(filtroCapturado);

        var cumpleFiltro = filtroCapturado.Compile();

        var mismoMedico = new Doctor("Dr. Test", "MP-123", specialtyId);
        var otroMedico = new Doctor("Dra. Test", "MP-456", Guid.NewGuid());

        Assert.True(cumpleFiltro(mismoMedico));
        Assert.False(cumpleFiltro(otroMedico));
    }

    [Fact]
    public async Task GetAll_CuandoCombinaNombreYEspecialidad_AplicaAmbosFiltros()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();

        Expression<Func<Doctor, bool>>? filtroCapturado = null;

        _persistence.Paginate<Doctor, string>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Do<Expression<Func<Doctor, bool>>>(f => filtroCapturado = f),
                Arg.Any<Expression<Func<Doctor, string>>>(),
                Arg.Any<string[]>())
            .Returns(Task.FromResult(
                new Pagination<Doctor>(10, 1, [], 0)));

        // Act
        await _service.GetAll(
            10,
            1,
            name: "Juan",
            specialtyId: specialtyId);

        // Assert
        Assert.NotNull(filtroCapturado);

        var cumpleFiltro = filtroCapturado.Compile();

        var cumpleAmbos = new Doctor("Juan Pérez", "MP-123", specialtyId);
        var nombreIncorrecto = new Doctor("Pedro Pérez", "MP-456", specialtyId);
        var especialidadIncorrecta = new Doctor("Juan Pérez", "MP-789", Guid.NewGuid());

        Assert.True(cumpleFiltro(cumpleAmbos));
        Assert.False(cumpleFiltro(nombreIncorrecto));
        Assert.False(cumpleFiltro(especialidadIncorrecta));
    }

    [Fact]
    public async Task GetAll_CuandoNoIndicaEspecialidad_NoFiltraPorEspecialidad()
    {
        // Arrange
        Expression<Func<Doctor, bool>>? filtroCapturado = null;

        _persistence.Paginate<Doctor, string>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Do<Expression<Func<Doctor, bool>>>(f => filtroCapturado = f),
                Arg.Any<Expression<Func<Doctor, string>>>(),
                Arg.Any<string[]>())
            .Returns(Task.FromResult(
                new Pagination<Doctor>(10, 1, [], 0)));

        // Act
        await _service.GetAll(10, 1);

        // Assert
        Assert.NotNull(filtroCapturado);

        var cumpleFiltro = filtroCapturado.Compile();

        var medico = new Doctor("Dr. Test", "MP-123", Guid.NewGuid());

        Assert.True(cumpleFiltro(medico));
    }

    [Fact]
    public async Task GetAll_CuandoElMedicoEstaInactivo_NoLoIncluye()
    {
        // Arrange
        Expression<Func<Doctor, bool>>? filtroCapturado = null;

        _persistence.Paginate<Doctor, string>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Do<Expression<Func<Doctor, bool>>>(f => filtroCapturado = f),
                Arg.Any<Expression<Func<Doctor, string>>>(),
                Arg.Any<string[]>())
            .Returns(Task.FromResult(
                new Pagination<Doctor>(10, 1, [], 0)));

        var doctor = new Doctor("Dr. Test", "MP-123", Guid.NewGuid());
        doctor.Deactivate();

        // Act
        await _service.GetAll(10, 1);

        // Assert
        Assert.NotNull(filtroCapturado);

        var cumpleFiltro = filtroCapturado.Compile();

        Assert.False(cumpleFiltro(doctor));
    }
}
