using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Dtos;

public record AuthenticatedUserDto
{
    public string AccessToken { get; init; }
    public UserInfoDto User { get; init; }
}

public record UserInfoDto
{
    public string Image { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string? Phone { get; init; }
    public string? PreferredLanguage { get; init; }
    public List<UserType> Roles { get; internal set; }
}
