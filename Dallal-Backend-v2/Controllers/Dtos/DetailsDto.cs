using Dallal_Backend_v2.Enums;

namespace Dallal_Backend_v2.Controllers.Dtos;

public class DetailsDefinitionDto
{
    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; } = new();
    public MultipleSearchBehavior Type { get; set; }
    public List<DetailsDefinitionOptionDto> Options { get; set; } = new();
}

public class DetailsDefinitionOptionDto
{
    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; } = new();
}
