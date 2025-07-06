using System.Text.Json;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Services;

public class SubmissionService(DatabaseContext _context)
{
    public async Task<Submission> UpsertSubmission<T>(
        SubmissionType type,
        Guid referenceId,
        T? initData,
        T newData
    )
        where T : class
    {
        var submission = await _context
            .Submissions.Where(s =>
                s.Type == type
                && s.ReferenceId == referenceId
                && s.Status == SubmissionStatus.Pending
            )
            .FirstOrDefaultAsync();

        var isNew = submission == null;

        submission ??= new Submission()
        {
            Id = Guid.NewGuid(),
            ReferenceId = referenceId,
            Type = type,
            Status = SubmissionStatus.Pending,
        };

        submission.SetOldValue(initData);
        submission.SetNewValue(newData);

        submission.ReferenceName = newData switch
        {
            Broker broker => broker.User.FirstName + " " + broker.User.LastName,
            Listing listing => listing.Name,
            _ => throw new NotImplementedException($"Unsupported type: {typeof(T).Name}"),
        };
        if (isNew)
            _context.Submissions.Add(submission);
        else
            _context.Submissions.Update(submission);
        return submission;
    }

    public async Task RejectSubmission(Guid id, string reason)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {id} not found.");
        }

        submission.Status = SubmissionStatus.Rejected;
        submission.RejectedAt = DateTime.UtcNow;
        submission.RejectedReason = reason;
        _context.Submissions.Update(submission);
        await _context.SaveChangesAsync();
    }

    public async Task CancelSubmission(Guid id)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {id} not found.");
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending submissions can be cancelled.");
        }

        submission.Status = SubmissionStatus.Cancelled;
        _context.Submissions.Update(submission);
        await _context.SaveChangesAsync();
    }

    public async Task ApproveSubmission(Guid id)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {id} not found.");
        }

        submission.Status = SubmissionStatus.Approved;
        submission.ApprovedAt = DateTime.UtcNow;
        _context.Submissions.Update(submission);
        await ApplyChanges(submission);
        await _context.SaveChangesAsync();
    }

    private async Task ApplyChanges(Submission submission)
    {
        if (submission.Type == SubmissionType.BrokerAccount)
            await ApplyChangesAndInsertInDb<Broker>(submission);
        else if (submission.Type == SubmissionType.Listing)
            await ApplyChangesAndInsertInDb<Listing>(submission);
        else
            throw new NotImplementedException($"Unsupported submission type: {submission.Type}");
    }

    private async Task ApplyChangesAndInsertInDb<T>(Submission submission)
        where T : class
    {
        T? reference = await _context.Set<T>().FindAsync(submission.ReferenceId);
        bool isNew = reference == null;
        reference = ApplyChanges<T>(submission, reference);
        if (isNew)
            _context.Set<T>().Add(reference);
        else
            _context.Set<T>().Update(reference);
    }

    public static T ApplyChanges<T>(Submission submission, T? reference)
        where T : class
    {
        reference ??= Activator.CreateInstance<T>();

        var changes = submission.GetChanges<T>();
        foreach (var change in changes)
            ApplyChange(reference, change);

        return reference;
    }

    private static void ApplyChange<T>(T reference, SubmissionChange change)
        where T : class
    {
        var property =
            reference.GetType().GetProperty(change.Field)
            ?? throw new InvalidOperationException(
                $"Property '{change.Field}' not found on type '{reference.GetType().Name}'."
            );

        if (change.NewValue != null)
            property.SetValue(
                reference,
                JsonSerializer.Deserialize(
                    change.NewValue,
                    property.PropertyType,
                    Submission.s_jsonOptions
                )
            );
        else
            property.SetValue(reference, null);
    }
}
