using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public record OAuthRequest
{
    public string idToken { get; init; } = default!;
    public UserType UserType { get; init; }
    public string PreferredLanguage { get; init; } = default!;
}
