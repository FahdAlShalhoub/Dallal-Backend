namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class DetailsDefinitionOptionDto
{
    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; } = new();
}