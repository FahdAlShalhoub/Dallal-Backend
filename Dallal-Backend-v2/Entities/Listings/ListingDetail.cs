using Dallal_Backend_v2.Entities.Details;

namespace Dallal_Backend_v2.Entities;

public class ListingDetail
{
    public Guid Id { get; set; }
    public DetailsDefinition Definition { get; set; } = default!;
    public Guid DefinitionId { get; set; }
    public DetailsDefinitionOption? Option { get; set; } = default!;
    public Guid? OptionId { get; set; }
    public string? Value { get; set; }
}
