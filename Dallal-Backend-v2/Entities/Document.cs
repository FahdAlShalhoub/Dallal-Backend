using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dallal_Backend_v2.Entities;

public class Document
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
        FileName = fileName;
        NameInBucket = nameInBucket;
        PlaceHolderNameInBucket = placeHolderNameInBucket;
    }

    public string? FileName { get; set; }
    public string NameInBucket { get; set; }
    public string? PlaceHolderNameInBucket { get; set; }
}
