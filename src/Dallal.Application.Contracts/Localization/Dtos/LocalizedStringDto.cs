using System.Collections.Generic;

namespace Dallal.Localization.Dtos;

public class LocalizedStringDto
{
    public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

    public LocalizedStringDto() { }

    public LocalizedStringDto(Dictionary<string, string> values)
    {
        Values = values ?? new Dictionary<string, string>();
    }
}
