using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record LoginAdminModel
{
    public record Request(
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        string Email,

         [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        string Password
    );

    public record Response(string? Token, string? Role);
}

public record LoginPatientModel
{
    public record Request(
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        string Email,

        [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener 7 u 8 dígitos")]
        long Dni
    );

    public record Response(string? Token, string? Role);
}