namespace Dallal_Backend_v2.Controllers.Submissions.Dtos;

public class SubmissionChangeDto
{
    public string Field { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
