using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
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
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => d.IsActive && (string.IsNullOrWhiteSpace(name) ||
                                                   d.Name.Contains(name)), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber, d.IsActive,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> CreateAsync(DoctorModel.Request request)
    {
        // 1. Validar que la especialidad exista
        if (request.SpecialityId != Guid.Empty)
        {
            var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
            if (speciality == null)
            {
                throw new Exception("La especialidad ingresada no existe."); 
            }
        }

        // 2. Mapeo de entrada
        var doctor = new Doctor(request.Name, request.LicenseNumber, request.SpecialityId);

        // 3. Guardamos en la base de datos
        await _persistence.Add(doctor);

        // 4. Mapeo de salida
        return new DoctorModel.Response
       (
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            doctor.IsActive,
            new DoctorModel.SpecialityDto(doctor.SpecialityId, "Pendiente")
        );
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        // 1. Buscamos al médico por su Id
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor == null) return false;

        // 2. Aplicamos tu borrado lógico usando el método del dominio
        doctor.Deactivate();

        // 3. Actualizamos en la base de datos
        await _persistence.Update(doctor);

        return true;
    }
}
