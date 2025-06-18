using System.Linq.Expressions;
using Dallal_Backend_v2.Controllers.Submissions.Dtos;
using Dallal_Backend_v2.Entities.Submissions;

namespace Dallal_Backend_v2.Helpers.EntityDtoMappers;

public static class SubmissionMapper
{
    public static Expression<Func<Submission, SubmissionDto>> SelectToDto() =>
        submission => new SubmissionDto
        {
            Id = submission.Id,
            Type = submission.Type,
            Status = submission.Status,
            CreatedAt = submission.CreatedAt,
            ApprovedAt = submission.ApprovedAt,
            RejectedAt = submission.RejectedAt,
            RejectedReason = submission.RejectedReason,
            ReferenceId = submission.ReferenceId,
            Changes = submission
                .Changes.Select(change => new SubmissionChangeDto
                {
                    Field = change.Field,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                })
                .ToList(),
        };
}
