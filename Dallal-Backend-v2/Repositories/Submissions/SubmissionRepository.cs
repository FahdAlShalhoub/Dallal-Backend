using Dallal_Backend_v2.Entities.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Submissions;

public class SubmissionRepository(DatabaseContext context)
    : Repository<Submission>(context),
        ISubmissionRepository
{
    public async Task<ItemsAndTotalCount<Submission>> GetSubmissionsAsync(
        SubmissionType? type,
        SubmissionStatus? status,
        int page,
        int pageSize
    ) =>
        await GetPaginatedAsync(
            page,
            pageSize,
            predicate: submission =>
                (!type.HasValue || submission.Type == type)
                && (!status.HasValue || submission.Status == status)
        );

    public async Task<Submission?> GetSubmissionByTypeAndReferenceIdAsync(
        SubmissionType type,
        Guid referenceId
    )
    {
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.Type == type && s.ReferenceId == referenceId
        );
    }

    public async Task<ItemsAndTotalCount<Submission>> GetSubmissionsByTypeAndStatusAsync(
        SubmissionType type,
        SubmissionStatus status,
        int page,
        int pageSize
    )
    {
        return await GetPaginatedAsync(
            page,
            pageSize,
            predicate: s => s.Type == type && s.Status == status,
            orderBy: q => q.OrderByDescending(s => s.CreatedAt)
        );
    }
}
