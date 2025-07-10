using Dallal_Backend_v2.Entities;

namespace Dallal_Backend_v2.Repositories;

public interface IAreaRepository : IRepository<Area>
{
    Task<IEnumerable<Area>> GetByParentIdAsync(Guid? parentId);
    Task<IEnumerable<Area>> SearchByNameAsync(string name);
    Task<IEnumerable<Area>> GetPaginatedAsync(int page, int pageSize);
    Task<Area?> GetByFullNameAsync(string fullName);
    Task<bool> ExistsByFullNameAsync(string fullName);
}