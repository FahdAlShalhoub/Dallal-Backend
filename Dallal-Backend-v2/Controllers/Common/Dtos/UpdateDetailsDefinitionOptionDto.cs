namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class UpdateDetailsDefinitionOptionDto
{
    public Guid? Id { get; set; } // Null for new options, set for existing options
    public LocalizedStringDto Name { get; set; } = new();
}