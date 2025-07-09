using Dallal_Backend_v2.Entities.Details;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class DetailsDefinitionDto
{
    public DetailsDefinitionDto(DetailsDefinition definition)
    {
        Id = definition.Id;
        Name = new LocalizedStringDto(definition.Name);
        Type = definition.Type;
        SearchBehavior = definition.SearchBehavior;
        PropertyTypes = definition.PropertyTypes?.ToList();
        IsHidden = definition.IsHidden;
        IsHiddenInSearch = definition.IsHiddenInSearch;
        DisplayCategory = definition.DisplayCategory;
        Options = definition
            .Options?.Select(o => new DetailsDefinitionOptionDto
            {
                Id = o.Id,
                Name = new LocalizedStringDto(o.Name),
            })
            .ToList();
    }

    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; } = new();
    public DetailDefinitionType Type { get; set; }
    public DetailDefinitionSearchBehavior SearchBehavior { get; set; }
    public List<PropertyType>? PropertyTypes { get; set; } = new();
    public bool IsHidden { get; set; }
    public bool IsHiddenInSearch { get; set; }
    public List<DetailsDefinitionOptionDto>? Options { get; set; } = new();
    public DetailDisplayCategory DisplayCategory { get; set; }
}