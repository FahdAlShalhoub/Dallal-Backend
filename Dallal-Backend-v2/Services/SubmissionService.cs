using System.Reflection;
using Dallal_Backend_v2.Entities.Submissions;

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

    private static List<SubmissionChange> GetChanges<T>(T? initData, T? newData)
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
            if (property.DeclaringType!.IsClass)
            {
                // TODO
                throw new NotImplementedException(
                    $"Property {property.Name} is not a primitive type"
                );
            }
            var initValue = property.GetValue(initData);
            var newValue = property.GetValue(newData);
            if (initValue != newValue)
            {
                changes.Add(
                    new SubmissionChange
                    {
                        Field = property.Name,
                        OldValue = initValue?.ToString(),
                        NewValue = newValue?.ToString(),
                    }
                );
            }
        }
        return changes;
    }
}
