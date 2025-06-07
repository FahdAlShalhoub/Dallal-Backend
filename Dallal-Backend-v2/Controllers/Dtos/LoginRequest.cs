using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Dtos;

public record LoginRequest
{
    public string Email { get; init; }
    public string Password { get; init; }
    public UserType UserType { get; init; }
}
