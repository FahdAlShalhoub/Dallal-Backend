using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Entities.Details;

public class DetailsDefinition
{
    public Guid Id { get; set; }
    public LocalizedString Name { get; set; } = default!;
    public DetailDefinitionType Type { get; set; }
    public DetailDefinitionSearchBehavior SearchBehavior { get; set; }
    public List<DetailsDefinitionOption>? Options { get; set; } = [];
    public List<PropertyType>? PropertyTypes { get; set; } = [];
    public bool IsRequired { get; set; }
    public bool IsHidden { get; set; }
    public bool IsHiddenInSearch
    {
        get => SearchBehavior == DetailDefinitionSearchBehavior.hidden || IsHidden;
        private set { }
    }
}
