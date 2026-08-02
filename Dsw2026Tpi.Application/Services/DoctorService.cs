using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => d.IsActive &&
            (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber, d.IsActive,
            new DoctorModel.SpecialtyDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> CreateAsync(DoctorModel.Request request)
    {
        var speciality = await _persistence.GetById<Speciality>(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        if (!speciality.IsActive)
            throw new EntityNotFoundException(nameof(Speciality)); 

        var doctor = new Doctor(request.Name, request.LicenseNumber, request.SpecialtyId);
        await _persistence.Add(doctor);

        return new DoctorModel.Response(
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            doctor.IsActive,
            new DoctorModel.SpecialtyDto(speciality.Id, speciality.Name)
        );
    }

    public async Task<DoctorModel.Response> UpdateAsync(Guid id, DoctorModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        if (!speciality.IsActive)
            throw new EntityNotFoundException(nameof(Speciality)); // punto 11

        doctor.Update(request.Name, request.LicenseNumber, request.SpecialtyId);
        await _persistence.Update(doctor);

        return new DoctorModel.Response(
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            doctor.IsActive,
            new DoctorModel.SpecialtyDto(speciality.Id, speciality.Name)
        );
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor == null) return false;

        doctor.Deactivate();

        await _persistence.Update(doctor);

        return true;
    }
}
