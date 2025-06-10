using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers.Submissions;

[Authorize(Roles = "Admin")]
[Route("submissions")]
public class AdminSubmissionController(
    SubmissionService _submissionService,
    DatabaseContext _context
) : DallalController
{
    [HttpGet]
    public async Task<List<SubmissionDto>> GetSubmissions(
        [FromQuery] SubmissionType type,
        [FromQuery] SubmissionStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var submissions = _context
            .Submissions.Where(s => s.Type == type && s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return submissions
            .Select(s => new SubmissionDto
            {
                Id = s.Id,
                Type = s.Type,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                Changes = s
                    .Changes.Select(c => new SubmissionChangeDto
                    {
                        Field = c.Field,
                        OldValue = c.OldValue,
                        NewValue = c.NewValue,
                    })
                    .ToList(),
            })
            .ToList();
    }

    [HttpPost("{id}/approve")]
    public async Task ApproveSubmission(Guid id)
    {
        await _submissionService.ApproveSubmission(id);
    }

    [HttpPost("{id}/reject")]
    public async Task RejectSubmission(Guid id, string reason)
    {
        await _submissionService.RejectSubmission(id, reason);
    }
}

public class SubmissionDto
{
    public Guid Id { get; set; }
    public SubmissionType Type { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SubmissionChangeDto> Changes { get; set; } = new();
}

public class SubmissionChangeDto
{
    public string Field { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
