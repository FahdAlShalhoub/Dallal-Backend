using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class CreateDetailsDefinitionDto
{
    public LocalizedStringDto Name { get; set; } = new();
    public DetailDefinitionType Type { get; set; }
    public DetailDefinitionSearchBehavior SearchBehavior { get; set; }
    public List<PropertyType> PropertyTypes { get; set; } = new();
    public bool IsHidden { get; set; }
    public List<CreateDetailsDefinitionOptionDto> Options { get; set; } = new();
}