using System.Text.Json;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Repositories;
using Dallal_Backend_v2.Repositories.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dallal_Backend_v2.Services;

public class SubmissionService(
    ISubmissionRepository _submissionRepository,
    IServiceProvider _serviceProvider
)
{
    public async Task<Submission> UpsertSubmission<T>(
        SubmissionType type,
        Guid referenceId,
        T? initData,
        T newData
    )
        where T : class
    {
        var submission = await _submissionRepository.FirstOrDefaultAsync(s =>
            s.Type == type && s.ReferenceId == referenceId && s.Status == SubmissionStatus.Pending
        );

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
            await _submissionRepository.AddAsync(submission);
        else
            await _submissionRepository.UpdateAsync(submission);
        return submission;
    }

    public async Task RejectSubmission(Guid id, string reason)
    {
        var submission = await _submissionRepository.GetAsync(id);
        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {id} not found.");
        }

        submission.Status = SubmissionStatus.Rejected;
        submission.RejectedAt = DateTime.UtcNow;
        submission.RejectedReason = reason;
        await _submissionRepository.UpdateAsync(submission);
    }

    public async Task CancelSubmission(Guid id)
    {
        var submission = await _submissionRepository.GetAsync(id);
        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {id} not found.");
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending submissions can be cancelled.");
        }

        submission.Status = SubmissionStatus.Cancelled;
        await _submissionRepository.UpdateAsync(submission);
    }

    public async Task ApproveSubmission(Guid id)
    {
        var submission = await _submissionRepository.GetAsync(id);
        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {id} not found.");
        }

        submission.Status = SubmissionStatus.Approved;
        submission.ApprovedAt = DateTime.UtcNow;
        await _submissionRepository.UpdateAsync(submission);
        await ApplyChanges(submission);
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
        where T : BaseEntity
    {
        var repository = _serviceProvider.GetRequiredService<IRepository<T>>();

        // Use FirstOrDefaultAsync with predicate since entities use Guid IDs
        T? reference = await repository.FirstOrDefaultAsync(entity =>
            EF.Property<Guid>(entity, "Id") == submission.ReferenceId
        );
        bool isNew = reference == null;

        reference = ApplyChanges<T>(submission, reference);

        if (isNew)
            await repository.AddAsync(reference);
        else
            await repository.UpdateAsync(reference);
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
