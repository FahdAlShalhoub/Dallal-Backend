namespace Dallal_Backend_v2.Controllers.Dtos;

public class DocumentDto
{
    public string? FileName { get; set; } = default!;
    public string NameInBucket { get; set; } = default!;
    public string? PlaceHolderNameInBucket { get; set; }
    public string ViewUrl { get; set; } = default!;
    public string? PlaceHolderViewUrl { get; set; }
}

public class CreateDocumentDto
{
    public string? FileName { get; set; }
    public required string NameInBucket { get; set; }
    public string? PlaceHolderNameInBucket { get; set; }
}
