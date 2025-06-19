using Dallal_Backend_v2.Controllers.Submissions.Dtos;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var submissions = await _context
            .Submissions.Where(s => s.Type == type && s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            // .Select(SubmissionMapper.SelectToDto())
            .ToListAsync();

        return submissions.Select(SubmissionMapper.SelectToDto().Compile()).ToList();
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
