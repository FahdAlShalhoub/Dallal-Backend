using Dallal_Backend_v2.Entities.Enums;

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

// New incoming DTOs for create and update operations
public class CreateDetailsDefinitionDto
{
    public LocalizedStringDto Name { get; set; } = new();
    public MultipleSearchBehavior Type { get; set; }
    public List<CreateDetailsDefinitionOptionDto> Options { get; set; } = new();
}

public class CreateDetailsDefinitionOptionDto
{
    public LocalizedStringDto Name { get; set; } = new();
}

public class UpdateDetailsDefinitionDto
{
    public LocalizedStringDto Name { get; set; } = new();
    public MultipleSearchBehavior Type { get; set; }
    public List<UpdateDetailsDefinitionOptionDto> Options { get; set; } = new();
}

public class UpdateDetailsDefinitionOptionDto
{
    public Guid? Id { get; set; } // Null for new options, set for existing options
    public LocalizedStringDto Name { get; set; } = new();
}
