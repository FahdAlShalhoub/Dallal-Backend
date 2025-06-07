using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Dallal_Backend_v2.ThirdParty;

public class S3
{
    private string ContainerName { get; set; }
    private string Region { get; set; }
    private string SecretAccessKey { get; set; }
    private string AccessKeyId { get; set; }
    private readonly AmazonS3Client _client;

    public S3(string containerName, string region, string secretAccessKey, string accessKeyId)
    {
        ContainerName = containerName;
        Region = region;
        SecretAccessKey = secretAccessKey;
        AccessKeyId = accessKeyId;

        var regionEndpoint = new AmazonS3Config {ServiceURL = region, ForcePathStyle = true};

        if (
            !string.IsNullOrWhiteSpace(accessKeyId)
            && !string.IsNullOrWhiteSpace(secretAccessKey)
        )
        {
            var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
            _client = new AmazonS3Client(credentials, regionEndpoint);
        }
        else
        {
            _client = new AmazonS3Client(regionEndpoint);
        }
    }

    public async Task<PresignedUrlDto> GetPresignedUrl(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
        }

        // Generate a unique file key to avoid conflicts
        var guid = Guid.NewGuid();
        var fileKey = $"documents/{guid}/{fileName}";
        var guidFileName = $"{guid}_{fileName}";

        // Create the presigned URL request
        var request = new GetPreSignedUrlRequest
        {
            BucketName = ContainerName,
            Key = fileKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(60), // URL expires in 60 minutes
            ContentType = GetContentType(fileName),
        };

        // Generate and return the presigned URL
        var presignedUrl = await _client.GetPreSignedURLAsync(request);

        return new PresignedUrlDto {Url = presignedUrl, FileName = guidFileName};
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream",
        };
    }
}

public record PresignedUrlDto
{
    public string Url { get; set; }
    public string FileName { get; set; }
}