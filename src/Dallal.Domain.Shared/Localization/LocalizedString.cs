using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace Dallal.Localization;

public class LocalizedString
{
    public Dictionary<string, string> Values { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string GetValue(string? languageCode = null)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            languageCode = Thread.CurrentThread.CurrentUICulture.Name;
        }

        if (Values.TryGetValue(languageCode, out var exactMatch))
        {
            return exactMatch;
        }

        var culture = new CultureInfo(languageCode);
        var parentCulture = culture.Parent;
        if (parentCulture != null && !string.IsNullOrEmpty(parentCulture.Name))
        {
            if (Values.TryGetValue(parentCulture.Name, out var parentMatch))
            {
                return parentMatch;
            }

            if (
                !string.Equals(
                    parentCulture.TwoLetterISOLanguageName,
                    parentCulture.Name,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                if (
                    Values.TryGetValue(
                        parentCulture.TwoLetterISOLanguageName,
                        out var twoLetterMatch
                    )
                )
                {
                    return twoLetterMatch;
                }
            }
        }

        var twoLetter = culture.TwoLetterISOLanguageName;
        if (
            !string.IsNullOrEmpty(twoLetter)
            && Values.TryGetValue(twoLetter, out var twoLetterValue)
        )
        {
            return twoLetterValue;
        }

        return Values.FirstOrDefault().Value;
    }

    public void SetValue(string languageCode, string value)
    {
        if (string.IsNullOrEmpty(languageCode))
            return;

        Values[languageCode] = value;
    }

    public bool Contains(string searchValue)
    {
        return Values.Values.Any(v => v.ToLower().Contains(searchValue.ToLower()));
    }

    public override string ToString()
    {
        return GetValue();
    }
}
