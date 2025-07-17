using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;

namespace Dallal_Backend_v2.Repositories.Submissions;

public interface ISubmissionRepository : IRepository<Submission>
{
    Task<ItemsAndTotalCount<Submission>> GetSubmissionsAsync(
        SubmissionType? type,
        SubmissionStatus? status,
        int page,
        int pageSize
    );
    Task<Submission?> GetSubmissionByTypeAndReferenceIdAsync(SubmissionType type, Guid referenceId);
    Task<ItemsAndTotalCount<Submission>> GetSubmissionsByTypeAndStatusAsync(
        SubmissionType type,
        SubmissionStatus status,
        int page,
        int pageSize
    );
}
