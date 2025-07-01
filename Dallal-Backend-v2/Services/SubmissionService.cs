using System.Collections;
using System.Reflection;
using System.Text.Json;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Services;

public class SubmissionService(DatabaseContext _context)
{
    private static readonly Dictionary<Type, PropertyInfo[]> _propertiesCache = [];
    public static readonly JsonSerializerOptions s_jsonOptions = CreateJsonOptions();

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

        submission.OldData = initData != null ? JsonSerializer.Serialize(initData, s_jsonOptions) : null;
        submission.NewData = JsonSerializer.Serialize(newData, s_jsonOptions);

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
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine(_context.ChangeTracker.DebugView.LongView);
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        await _context.SaveChangesAsync();
    }


    private static JsonSerializerOptions CreateJsonOptions()
    {
        var _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
        return _jsonOptions;
    }

    private async Task ApplyChanges(Submission submission)
    {
        object? reference;

        if (submission.Type == SubmissionType.BrokerAccount)
            reference = await _context
                .Set<Broker>()
                .FirstOrDefaultAsync(i => i.Id == submission.ReferenceId);
        else if (submission.Type == SubmissionType.Listing)
            reference = await _context
                .Set<Listing>()
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == submission.ReferenceId);
        else
            throw new NotImplementedException();
        bool isNew = reference == null;
        Console.WriteLine(
            $"Applying changes to reference of type: {reference?.GetType().Name} (ID: {submission.ReferenceId}) {(isNew ? "new" : "existing")}"
        );
        Console.WriteLine(JsonSerializer.Serialize(reference, s_jsonOptions));
        reference = ApplyChanges(submission, reference);

        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine(_context.ChangeTracker.DebugView.LongView);
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        Console.WriteLine("===============================================");
        if (isNew)
            _context.Add(reference);
        else if (reference is Broker broker)
            _context.Set<Broker>().Update(broker);
        else if (reference is Listing listing)
            _context.Set<Listing>().Update(listing);
    }

    public static object ApplyChanges(Submission submission, object? reference)
    {
        reference ??= Activator.CreateInstance(
            submission.Type switch
            {
                SubmissionType.BrokerAccount => typeof(Broker),
                SubmissionType.Listing => typeof(Listing),
                _ => throw new NotImplementedException(),
            }
        )!;

        // var isNull = (reference == null) ? "null" : "not null";
        // Console.WriteLine(
        //     $"Applying changes to reference of type: {reference?.GetType().Name} (ID: {submission.ReferenceId}) {isNull}"
        // );

        var changes = submission.GetChanges<object>();
        foreach (var change in changes)
        {
            ApplyChange(reference, change);
        }

        return reference;
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
        Console.WriteLine(
            $"Applying change to property: {property.Name} (Type: {property.PropertyType.Name})"
        );
        Console.WriteLine(
            $"Old Value: {change.OldValue ?? "null"}, New Value: {change.NewValue ?? "null"}"
        );
        if (change.NewValue != null)
            property.SetValue(
                reference,
                JsonSerializer.Deserialize(change.NewValue, property.PropertyType, s_jsonOptions)
            );
        else
            property.SetValue(reference, null);
    }
}
