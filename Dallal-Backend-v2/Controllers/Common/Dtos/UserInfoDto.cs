using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public record UserInfoDto
{
    public DocumentDto? Image { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? PreferredLanguage { get; init; }
    public List<UserType> Roles { get; init; } = [];
}