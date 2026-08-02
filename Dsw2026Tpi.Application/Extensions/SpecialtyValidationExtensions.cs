using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class SpecialtyValidationExtensions
    {
        public static void Validate(this SpecialityModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 ||
                request.Name.Length > 100)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("name", "Debe tener entre 3 y 100 caracteres");
            
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 ||
                request.Description.Length > 100)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("description", "Debe tener entre 10 y 100 caracteres");
        }
    }
}
