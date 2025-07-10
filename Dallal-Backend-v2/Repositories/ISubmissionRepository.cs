using Dallal_Backend_v2.Entities.Submissions;

namespace Dallal_Backend_v2.Repositories;

public interface ISubmissionRepository : IRepository<Submission>
{
    Task<IEnumerable<Submission>> GetByBrokerIdAsync(int brokerId);
    Task<IEnumerable<Submission>> GetByListingIdAsync(int listingId);
    Task<Submission?> GetDetailedByIdAsync(Guid id);
    Task<Submission?> GetByIdAsync(Guid id);
    Task<IEnumerable<Submission>> GetPaginatedAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
}