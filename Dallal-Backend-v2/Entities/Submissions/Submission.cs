using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Services;

namespace Dallal_Backend_v2.Entities.Submissions;

public class Submission : BaseEntity
{
    public static readonly JsonSerializerOptions s_jsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
        _jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        return _jsonOptions;
    }

    public SubmissionType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public string? OldData { get; private set; }

    [Column(TypeName = "jsonb")]
    public string NewData { get; private set; } = default!;

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

        return JsonSerializer.Deserialize<T>(OldData, s_jsonOptions);
    }

    public T? GetNewValue<T>()
    {
        if (string.IsNullOrEmpty(NewData))
            return default;

        return JsonSerializer.Deserialize<T>(NewData, s_jsonOptions);
    }

    public void SetOldValue<T>(T? value)
    {
        OldData = value != null ? JsonSerializer.Serialize(value, s_jsonOptions) : null;
    }

    public void SetNewValue<T>(T value)
    {
        NewData = JsonSerializer.Serialize(value, s_jsonOptions);
    }

    public List<SubmissionChange> GetChanges()
    {
        if (Type == SubmissionType.BrokerAccount)
            return GetChanges<Broker>();
        if (Type == SubmissionType.Listing)
            return GetChanges<Listing>();
        throw new NotImplementedException($"Unsupported submission type: {Type}");
    }

    public List<SubmissionChange> GetChanges<T>()
        where T : class
    {
        var oldValue = GetOldValue<T>();
        var newValue = GetNewValue<T>();

        if (newValue == null)
            return new List<SubmissionChange>();

        return CalculateChanges(oldValue, newValue);
    }

    public List<SubmissionChange> CalculateChanges<T>(T? oldObj, T? newObj)
    {
        var changes = new List<SubmissionChange>();

        if (newObj == null)
            return changes;

        var type = newObj.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            if (
                property.GetCustomAttributes(typeof(DoNotIncludeInSubmissionAttribute), false).Any()
            )
                continue;

            // var fieldName = string.IsNullOrEmpty(prefix)
            //     ? property.Name
            //     : $"{prefix}.{property.Name}";

            var fieldName = property.Name;

            var oldValue = oldObj != null ? property.GetValue(oldObj) : null;
            var newValue = property.GetValue(newObj);

            if (IsListProperty(property.PropertyType))
            {
                if (!AreListsEqual(oldValue, newValue))
                {
                    changes.Add(
                        new SubmissionChange
                        {
                            Field = fieldName,
                            OldValue =
                                oldValue != null
                                    ? JsonSerializer.Serialize(oldValue, s_jsonOptions)
                                    : null,
                            NewValue =
                                newValue != null
                                    ? JsonSerializer.Serialize(newValue, s_jsonOptions)
                                    : null,
                        }
                    );
                }
            }
            else if (!Equals(oldValue, newValue))
            {
                string v = JsonSerializer.Serialize(oldValue, s_jsonOptions);
                string v1 = JsonSerializer.Serialize(newValue, s_jsonOptions);
                if (v == v1)
                    continue;
                changes.Add(
                    new SubmissionChange
                    {
                        Field = fieldName,
                        OldValue = oldValue != null ? v : null,
                        NewValue = newValue != null ? v1 : null,
                    }
                );
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
        return !type.IsPrimitive
            && !type.IsEnum
            && type != typeof(string)
            && type != typeof(DateTime)
            && type != typeof(DateTime?)
            && type != typeof(Guid)
            && type != typeof(Guid?)
            && type != typeof(decimal)
            && type != typeof(decimal?)
            && type != typeof(double)
            && type != typeof(double?)
            && type != typeof(float)
            && type != typeof(float?)
            && type != typeof(int)
            && type != typeof(int?)
            && type != typeof(long)
            && type != typeof(long?)
            && type != typeof(bool)
            && type != typeof(bool?)
            && !IsListProperty(type);
    }

    private static bool AreListsEqual(object? list1, object? list2)
    {
        if (list1 == null && list2 == null)
            return true;
        if (list1 == null || list2 == null)
            return false;

        var json1 = JsonSerializer.Serialize(list1, s_jsonOptions);
        var json2 = JsonSerializer.Serialize(list2, s_jsonOptions);
        return json1 == json2;
    }
}

public class SubmissionChange
{
    public string Field { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class DoNotIncludeInSubmissionAttribute : Attribute { }

public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled,
}

public enum SubmissionType
{
    BrokerAccount,
    Listing,
}
