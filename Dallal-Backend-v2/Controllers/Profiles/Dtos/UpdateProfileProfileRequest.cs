using Dallal_Backend_v2.Controllers.Common.Dtos;

namespace Dallal_Backend_v2.Controllers.Profiles.Dtos;

public class UpdateProfileProfileRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public CreateDocumentDto? Image { get; set; }
}
