using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

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

        return doctors.Map(d => d.ToResponse());
    }

    public async Task<DoctorModel.Response> CreateAsync(DoctorModel.Request request)
    {
        request.Name.ValidateName();

        var speciality = await _persistence.GetById<Speciality>(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        if (!speciality.IsActive)
            throw new EntityNotFoundException(nameof(Speciality)); 

        var doctor = new Doctor(request.Name, request.LicenseNumber, request.SpecialtyId);
        await _persistence.Add(doctor);

        return doctor.ToResponse(speciality);
    }

    public async Task<DoctorModel.Response> UpdateAsync(Guid id, DoctorModel.Request request)
    {
        request.Name.ValidateName();

        var doctor = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        if (!speciality.IsActive)
            throw new EntityNotFoundException(nameof(Speciality)); // punto 11

        doctor.Update(request.Name, request.LicenseNumber, request.SpecialtyId);
        await _persistence.Update(doctor);

        return doctor.ToResponse(speciality);
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
