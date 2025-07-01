namespace Dallal_Backend_v2.Controllers.Submissions.Dtos;

public class DetailedSubmissionDto : SummarySubmissionDto
{
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public List<SubmissionChangeDto> Changes { get; set; } = new();
}
