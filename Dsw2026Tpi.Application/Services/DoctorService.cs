using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Dtos.Doctors;
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
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => d.IsActive && (string.IsNullOrWhiteSpace(name) ||
                                                   d.Name.Contains(name)), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorResponseDto> CreateAsync(CreateDoctorDto dto)
    {
        // 1. Validar que la especialidad exista
        if (dto.SpecialityId.HasValue && dto.SpecialityId.Value != Guid.Empty)
        {
            var speciality = await _persistence.GetByIdAsync<Speciality>(dto.SpecialityId.Value);
            if (speciality == null)
            {
                throw new EntityNotFoundException(nameof(Doctor)); 
            }
        }

        // 2. Mapeo de entrada
        var doctor = new Doctor(dto.Name, dto.LicenseNumber, dto.SpecialityId );

        // 3. Guardamos en la base de datos
        await _persistence.Add(doctor);

        // 4. Mapeo de salida
        return new DoctorResponseDto
        {
            Id = doctor.Id,
            Name = doctor.Name,
            LicenseNumber = doctor.LicenseNumber,
            IsActive = doctor.IsActive,
            SpecialityId = doctor.SpecialityId ?? Guid.Empty,
            SpecialityName = "Pendiente" // Se actualizará cuando la base de datos cruce los datos
        };
    }

    public async Task<IEnumerable<DoctorResponseDto>> GetAllAsync()
    {
        var doctors = await _persistence.GetAll<Doctor>();

        if (doctors == null)
        {
            return new List<DoctorResponseDto>();
        }

        return doctors.Select(d => new DoctorResponseDto
        {
            Id = d.Id,
            Name = d.Name,
            LicenseNumber = d.LicenseNumber,
            IsActive = d.IsActive,
            SpecialityId = d.SpecialityId ?? Guid.Empty,
            SpecialityName = d.Speciality?.Name ?? "Sin Especialidad"
        }).ToList();
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
