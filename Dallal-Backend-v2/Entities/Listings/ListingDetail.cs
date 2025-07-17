using System.Text.Json.Serialization;
using Dallal_Backend_v2.Entities.Details;

namespace Dallal_Backend_v2.Entities;

public class ListingDetail : BaseEntity
{

    [JsonIgnore]
    public DetailsDefinition Definition { get; set; } = default!;
    public Guid DefinitionId { get; set; }

    [JsonIgnore]
    public DetailsDefinitionOption? Option { get; set; } = default!;
    public Guid? OptionId { get; set; }
    public string? Value { get; set; }
}
