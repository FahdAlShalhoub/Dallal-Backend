using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Dallal_Backend_v2.Services;

namespace Dallal_Backend_v2.Entities.Submissions;

public class Submission
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SubmissionType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public string? OldData { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string NewData { get; set; } = default!;

    public Guid ReferenceId { get; set; }
    public string? ReferenceName { get; set; }

    public SubmissionStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedReason { get; set; }

    public T? GetOldValue<T>()
    {
        if (string.IsNullOrEmpty(OldData))
            return default;

        return JsonSerializer.Deserialize<T>(OldData, SubmissionService.s_jsonOptions);
    }

    public T? GetNewValue<T>()
    {
        if (string.IsNullOrEmpty(NewData))
            return default;

        return JsonSerializer.Deserialize<T>(NewData, SubmissionService.s_jsonOptions);
    }

    public List<SubmissionChange> GetChanges<T>() where T : class
    {
        var oldValue = GetOldValue<T>();
        var newValue = GetNewValue<T>();
        
        if (newValue == null)
            return new List<SubmissionChange>();

        return CalculateChanges(oldValue, newValue, string.Empty);
    }

    private List<SubmissionChange> CalculateChanges(object? oldObj, object? newObj, string prefix)
    {
        var changes = new List<SubmissionChange>();

        if (newObj == null)
            return changes;

        var type = newObj.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            if (property.GetCustomAttributes(typeof(DoNotIncludeInSubmissionAttribute), false).Any())
                continue;

            var fieldName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            var oldValue = oldObj != null ? property.GetValue(oldObj) : null;
            var newValue = property.GetValue(newObj);

            if (IsListProperty(property.PropertyType))
            {
                if (!AreListsEqual(oldValue, newValue))
                {
                    changes.Add(new SubmissionChange
                    {
                        Field = fieldName,
                        OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue, SubmissionService.s_jsonOptions) : null,
                        NewValue = newValue != null ? JsonSerializer.Serialize(newValue, SubmissionService.s_jsonOptions) : null
                    });
                }
            }
            else if (IsComplexType(property.PropertyType))
            {
                changes.AddRange(CalculateChanges(oldValue, newValue, fieldName));
            }
            else if (!Equals(oldValue, newValue))
            {
                changes.Add(new SubmissionChange
                {
                    Field = fieldName,
                    OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue, SubmissionService.s_jsonOptions) : null,
                    NewValue = newValue != null ? JsonSerializer.Serialize(newValue, SubmissionService.s_jsonOptions) : null
                });
            }
        }

        return changes;
    }

    private static bool IsListProperty(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive && !type.IsEnum && type != typeof(string) && 
               type != typeof(DateTime) && type != typeof(DateTime?) &&
               type != typeof(Guid) && type != typeof(Guid?) &&
               type != typeof(decimal) && type != typeof(decimal?) &&
               type != typeof(double) && type != typeof(double?) &&
               type != typeof(float) && type != typeof(float?) &&
               type != typeof(int) && type != typeof(int?) &&
               type != typeof(long) && type != typeof(long?) &&
               type != typeof(bool) && type != typeof(bool?) &&
               !IsListProperty(type);
    }

    private static bool AreListsEqual(object? list1, object? list2)
    {
        if (list1 == null && list2 == null) return true;
        if (list1 == null || list2 == null) return false;

        var json1 = JsonSerializer.Serialize(list1, SubmissionService.s_jsonOptions);
        var json2 = JsonSerializer.Serialize(list2, SubmissionService.s_jsonOptions);
        return json1 == json2;
    }
}

public class SubmissionChange
{
    public string Field { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class DoNotIncludeInSubmissionAttribute : Attribute
{
}

public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
}

public enum SubmissionType
{
    BrokerAccount,
    Listing,
}
