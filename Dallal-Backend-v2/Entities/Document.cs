using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dallal_Backend_v2.Entities;

public record Document
{
    [JsonConstructor]
    private Document() { }

    public Document(string staticUrl)
    {
        FileName = staticUrl;
        NameInBucket = staticUrl;
        PlaceHolderNameInBucket = null;
    }

    public Document(string? fileName, string nameInBucket, string? placeHolderNameInBucket = null)
    {
        if (string.IsNullOrWhiteSpace(nameInBucket))
            throw new ArgumentException(
                "Name in bucket cannot be null or empty",
                nameof(nameInBucket)
            );
        FileName = fileName;
        NameInBucket = nameInBucket;
        PlaceHolderNameInBucket = placeHolderNameInBucket;
    }

    public string? FileName { get; set; }
    public string NameInBucket { get; set; } = default!;
    public string? PlaceHolderNameInBucket { get; set; }
}
