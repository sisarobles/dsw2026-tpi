namespace Dsw2026Tpi.Application.Dtos;

public record DoctorModel
{
    public record Request(string Name, string LicenseNumber, Guid SpecialtyId);
    public record Response(Guid Id, string Name, string LicenseNumber, bool IsActive, SpecialtyDto? Speciality);
    public record SpecialtyDto(Guid? SpecialityId, string? Name);
}
