using Dallal_Backend_v2.Entities;

namespace Dallal_Backend_v2.Repositories.Areas;

public interface IAreaRepository : IRepository<Area>
{
    Task<ItemsAndTotalCount<Area>> GetPaginatedAreasWithParentAsync(
        int page,
        int pageSize,
        string? search = null
    );
    Task<List<Area>> GetLeafAreasAsync(int page, int pageSize, string? search = null);
    Task<Dictionary<Guid, Area>> GetAreasByIdsAsync(List<Guid> areaIds);
}
