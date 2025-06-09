using System.Reflection;
using System.Text.Json;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Services;

public class SubmissionService(DatabaseContext _context)
{
    private static readonly Dictionary<Type, PropertyInfo[]> _propertiesCache = [];

    public async Task<Submission> CreateSubmission<T>(
        SubmissionType type,
        Guid referenceId,
        T initData,
        T newData
    )
        where T : class?
    {
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            ReferenceId = referenceId,
            Type = type,
            Status = SubmissionStatus.Pending,
        };
        submission.Changes = GetChanges(initData, newData);
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
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

    private static List<SubmissionChange> GetChanges<T>(T? initData, T? newData, string prefix = "")
        where T : class?
    {
        if (!_propertiesCache.TryGetValue(typeof(T), out var properties))
        {
            properties = typeof(T).GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy
            );
            _propertiesCache[typeof(T)] = properties;
        }

        var changes = new List<SubmissionChange>();
        foreach (var property in properties)
        {
            Console.WriteLine(
                $"Processing property: {property.Name} (Type: {property.PropertyType.Name}) {property.PropertyType.IsClass}"
            );
            // if (property.PropertyType)
            // {
            //     var initValue = initData == null ? null : property.GetValue(initData);
            //     var newValue = newData == null ? null : property.GetValue(newData);
            //     changes.AddRange(GetChanges(initValue, newValue, prefix + property.Name + "."));
            // }
            // else
            {
                var initValue = initData == null ? null : property.GetValue(initData);
                var newValue = newData == null ? null : property.GetValue(newData);
                if (initValue != newValue)
                {
                    changes.Add(
                        new SubmissionChange
                        {
                            Field = prefix + property.Name,
                            OldValue = JsonSerializer.Serialize(initValue),
                            NewValue = JsonSerializer.Serialize(newValue),
                        }
                    );
                }
            }
        }
        return changes;
    }

    private async Task ApplyChanges(Submission submission)
    {
        object reference;

        if (submission.Type == SubmissionType.BrokerAccount)
            reference = await _context
                .Set<Broker>()
                .FirstAsync(i => i.Id == submission.ReferenceId);
        else if (submission.Type == SubmissionType.Listing)
            reference = await _context
                .Set<Listing>()
                .FirstAsync(i => i.Id == submission.ReferenceId);
        else
            throw new NotImplementedException();

        foreach (var change in submission.Changes)
        {
            ApplyChange(reference, change);
        }

        _context.Update(reference);
    }

    private static void ApplyChange(object reference, SubmissionChange change)
    {
        if (change.Field.Contains("."))
        {
            var firstPart = change.Field.Split('.')[0];
            var subProperty =
                reference.GetType().GetProperty(firstPart)
                ?? throw new InvalidOperationException(
                    $"Property '{firstPart}' not found on type '{reference.GetType().Name}'."
                );
            var subReference =
                subProperty.GetValue(reference)
                ?? throw new InvalidOperationException(
                    $"Property '{firstPart}' is null on type '{reference.GetType().Name}'."
                );
            ApplyChange(
                subReference,
                new SubmissionChange
                {
                    Field = change.Field.Substring(firstPart.Length + 1),
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                }
            );
            return;
        }
        var property =
            reference.GetType().GetProperty(change.Field)
            ?? throw new InvalidOperationException(
                $"Property '{change.Field}' not found on type '{reference.GetType().Name}'."
            );
        if (change.NewValue != null)
            property.SetValue(
                reference,
                JsonSerializer.Deserialize(change.NewValue, property.PropertyType)
            );
        else
            property.SetValue(reference, null);
    }
}
