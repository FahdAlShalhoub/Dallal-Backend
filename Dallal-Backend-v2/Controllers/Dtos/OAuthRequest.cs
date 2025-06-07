using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Dtos;

public record OAuthRequest
{
    public string idToken { get; init; }
    public UserType UserType { get; init; }
}
