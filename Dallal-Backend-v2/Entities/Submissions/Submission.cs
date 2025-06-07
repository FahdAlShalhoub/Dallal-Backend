using System.ComponentModel.DataAnnotations.Schema;

namespace Dallal_Backend_v2.Entities.Submissions;

public class Submission
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SubmissionType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public List<SubmissionChange> Changes { get; set; } = default!;

    public Guid ReferenceId { get; set; }

    public SubmissionStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedReason { get; set; }
}

public class SubmissionChange
{
    public string Field { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
}

public enum SubmissionType
{
    BrokerAccount,
    Listing,
}
