using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record RegisterModel
{
    public record Request(
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        string Email,

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        string Password
    );

    public record Response(string Email);
}