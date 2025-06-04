namespace Dallal_Backend_v2.Controllers.Dtos;

public record AuthenticatedUser
{
    public string AccessToken { get; init; }
    public UserInfo User { get; init; }
}

public record UserInfo
{
    public string Image { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string Type { get; init; }
}