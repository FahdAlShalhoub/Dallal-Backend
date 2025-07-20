using Dallal_Backend_v2.Controllers.Areas.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;

namespace Dallal_Backend_v2.Helpers.EntityDtoMappers;

public static class AreaMapper
{
    public static AreaDto ToDto(Area area)
    {
        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            FullName = new LocalizedStringDto(area.FullName),
            CreatedAt = area.CreatedAt,
            Parent = area.Parent != null ? ToDto(area.Parent) : null,
        };
    }

    public static List<AreaDto> ToDto(IEnumerable<Area> areas)
    {
        return areas.Select(ToDto).ToList();
    }
}
