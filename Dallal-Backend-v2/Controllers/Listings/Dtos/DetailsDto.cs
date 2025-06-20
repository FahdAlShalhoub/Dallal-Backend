namespace Dallal_Backend_v2.Controllers.Listings.Dtos;

public class DetailsDto
{
    public Guid DefinitionId { get; set; }
    public Guid? OptionId { get; set; }
    public string? Value { get; set; }
}
