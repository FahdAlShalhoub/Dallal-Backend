using Dallal_Backend_v2.Entities.Submissions;

namespace Dallal_Backend_v2.Controllers.Submissions.Dtos;

public class SubmissionDto
{
    public Guid Id { get; set; }
    public SubmissionType Type { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedReason { get; set; }
    public Guid ReferenceId { get; set; }
    public List<SubmissionChangeDto> Changes { get; set; } = new();
}
