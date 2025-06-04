using System.ComponentModel;
using System.Reflection;

namespace Dallal_Backend_v2.Entities;

public enum UserTypes
{
    [Description("Buyer")]
    Buyer,
    [Description("Broker")]
    Broker
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
