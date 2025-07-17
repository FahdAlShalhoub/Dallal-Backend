using Dallal_Backend_v2.Controllers.Areas.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Repositories.Areas;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers.Areas;

[Route("areas")]
public class AreaController(IAreaRepository _areaRepository) : DallalController
{
    // list paginated areas
    [HttpGet]
    public async Task<PaginatedList<AreaDto>> GetAreas(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var result = await _areaRepository.GetPaginatedAreasWithParentAsync(page, pageSize, search);

        return new PaginatedList<AreaDto>(
            AreaMapper.ToDto(result.Items),
            page,
            pageSize,
            (int)result.Count
        );
    }

    // leaf areas only
    [HttpGet("leafs")]
    public async Task<List<AreaDto>> GetLeafAreas(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var areas = await _areaRepository.GetLeafAreasAsync(page, pageSize, search);

        return AreaMapper.ToDto(areas);
    }
}
