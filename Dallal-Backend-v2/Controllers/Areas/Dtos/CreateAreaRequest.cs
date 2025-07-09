using Dallal_Backend_v2.Controllers.Common.Dtos;

namespace Dallal_Backend_v2.Controllers.Areas.Dtos;

public class CreateAreaRequest
{
    public LocalizedStringDto Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
}