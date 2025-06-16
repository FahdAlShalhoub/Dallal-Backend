using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Dtos;

public record SignupRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }

    [Required]
    [MinLength(6)]
    public string Password { get; init; }

    public string? Name { get; init; }

    public string? ProfileImage { get; init; }

    [Required]
    public UserType UserType { get; init; }

    [Required]
    public string PreferredLanguage { get; init; }
}
