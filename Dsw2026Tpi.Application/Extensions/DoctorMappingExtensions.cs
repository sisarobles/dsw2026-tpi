using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class DoctorMappingExtensions
    {
        public static DoctorModel.Response ToResponse(this Doctor doctor)
        {
            return doctor.ToResponse(doctor.Speciality);
        }

        public static DoctorModel.Response ToResponse(this Doctor doctor, Speciality? speciality)
        {
            return new DoctorModel.Response(
                doctor.Id,
                doctor.Name,
                doctor.LicenseNumber,
                doctor.IsActive,
                new DoctorModel.SpecialtyDto(speciality?.Id, speciality?.Name)
            );
        }
    }
}
