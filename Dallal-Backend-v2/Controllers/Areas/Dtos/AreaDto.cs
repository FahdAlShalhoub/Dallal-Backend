using Dallal_Backend_v2.Controllers.Common.Dtos;

namespace Dallal_Backend_v2.Controllers.Areas.Dtos;

public class AreaDto
{
    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; } = default!;
    public LocalizedStringDto FullName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public AreaDto? Parent { get; set; }
}