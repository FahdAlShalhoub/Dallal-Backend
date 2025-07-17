namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public record AuthenticatedUserDto
{
    public string AccessToken { get; init; } = default!;
    public UserInfoDto User { get; init; } = default!;
}
