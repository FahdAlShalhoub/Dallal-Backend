namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class CreateDocumentDto
{
    public string? FileName { get; set; }
    public required string NameInBucket { get; set; }
    public string? PlaceHolderNameInBucket { get; set; }
}