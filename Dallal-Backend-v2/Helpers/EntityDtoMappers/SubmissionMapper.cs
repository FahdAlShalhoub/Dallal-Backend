using System.Linq.Expressions;
using Dallal_Backend_v2.Controllers.Submissions.Dtos;
using Dallal_Backend_v2.Entities.Submissions;

namespace Dallal_Backend_v2.Helpers.EntityDtoMappers;

public static class SubmissionMapper
{
    public static Expression<Func<Submission, SummarySubmissionDto>> SelectToDto() =>
        submission => new SummarySubmissionDto
        {
            Id = submission.Id,
            Type = submission.Type,
            Status = submission.Status,
            CreatedAt = submission.CreatedAt,
            ApprovedAt = submission.ApprovedAt,
            RejectedAt = submission.RejectedAt,
            RejectedReason = submission.RejectedReason,
            ReferenceId = submission.ReferenceId,
            ReferenceName = submission.ReferenceName,
        };
}
