using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Controllers.Submissions.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Helpers;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Submissions;

[Authorize(Roles = "Admin")]
[Route("submissions")]
public class AdminSubmissionController(
    SubmissionService _submissionService,
    DatabaseContext _context,
    S3 _s3Service
) : DallalController
{
    [HttpGet]
    public async Task<PaginatedList<SummarySubmissionDto>> GetSubmissions(
        [FromQuery] SubmissionType? type,
        [FromQuery] SubmissionStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var submissions = await _context
            .Submissions.WhereIf(type.HasValue, s => s.Type == type!.Value)
            .WhereIf(status.HasValue, s => s.Status == status!.Value)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            // .Select(SubmissionMapper.SelectToDto())
            .ToListAsync();

        var count = await _context.Submissions.CountAsync(s =>
            s.Type == type && s.Status == status
        );

        return new PaginatedList<SummarySubmissionDto>(
            submissions.Select(SubmissionMapper.SelectToDto().Compile()).ToList(),
            page,
            count,
            pageSize
        );
    }

    [HttpGet("{id}")]
    public async Task<DetailedSubmissionDto> GetSubmission(Guid id)
    {
        var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);

        if (submission == null)
            throw new EntityNotFoundException(typeof(Submission), id);

        object? oldDataDto = submission.Type switch
        {
            SubmissionType.BrokerAccount => await BrokerMapper.GetDtoFromSubmission(
                await _context
                    .Users.Where(u => u.Id == UserId && u.Broker != null)
                    .Include(u => u.Broker)
                    .FirstAsync(),
                submission.GetOldValue<Broker>(),
                _s3Service
            ),
            SubmissionType.Listing => await ListingMapper.MapToDto(
                submission.GetOldValue<Listing>(),
                _context,
                _s3Service
            ),
            _ => throw new NotImplementedException(),
        };

        object? newDataDto = submission.Type switch
        {
            SubmissionType.BrokerAccount => await BrokerMapper.GetDtoFromSubmission(
                await _context
                    .Users.Where(u => u.Id == UserId && u.Broker != null)
                    .Include(u => u.Broker)
                    .FirstAsync(),
                submission.GetNewValue<Broker>(),
                _s3Service
            ),
            SubmissionType.Listing => await ListingMapper.MapToDto(
                submission.GetNewValue<Listing>(),
                _context,
                _s3Service
            ),
            _ => throw new NotImplementedException(),
        };

        var submissionChangeDtos = submission
            .CalculateChanges(oldDataDto, newDataDto)
            .Select(c => new SubmissionChangeDto
            {
                Field = c.Field,
                OldValue = c.OldValue,
                NewValue = c.NewValue,
            })
            .ToList();

        return new DetailedSubmissionDto
        {
            Id = submission.Id,
            Type = submission.Type,
            Status = submission.Status,
            ReferenceId = submission.ReferenceId,
            ReferenceName = submission.ReferenceName,
            CreatedAt = submission.CreatedAt,
            ApprovedAt = submission.ApprovedAt,
            RejectedAt = submission.RejectedAt,
            RejectedReason = submission.RejectedReason,
            Changes = submissionChangeDtos,
            OldValue = oldDataDto,
            NewValue = newDataDto,
        };
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

    [HttpPost("{id}/cancel")]
    public async Task CancelSubmission(Guid id)
    {
        await _submissionService.CancelSubmission(id);
    }
}
