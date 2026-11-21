using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class DoctorValidationExtensions
    {
        public static void ValidateName(this string? name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100)
            {
                throw new ValidationException(
                    ErrorCodes.VALIDATION_ERROR,
                    nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("Name", "El nombre debe tener entre 3 y 100 caracteres");
            }
        }

        public static void ValidateLicenseNumber(this string? licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber) || licenseNumber.Length > 50)
            {
                throw new ValidationException(
                    ErrorCodes.VALIDATION_ERROR,
                    nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail(
                        "LicenseNumber",
                        "La matrícula profesional es obligatoria y no puede superar los 50 caracteres");
            }
        }
    }
}
