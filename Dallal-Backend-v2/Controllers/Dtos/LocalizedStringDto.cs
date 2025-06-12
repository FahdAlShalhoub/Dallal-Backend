using Dallal_Backend_v2.Entities;

namespace Dallal_Backend_v2.Controllers.Dtos;

public class LocalizedStringDto
{
    public Dictionary<string, string> Values { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public LocalizedStringDto() { }

    public LocalizedStringDto(string value)
    {
        Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "fallback", value },
        };
    }

    public LocalizedStringDto(LocalizedString localizedString)
    {
        Values = localizedString.Values.ToDictionary();
    }
}
