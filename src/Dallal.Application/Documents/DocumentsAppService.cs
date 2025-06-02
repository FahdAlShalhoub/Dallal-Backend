using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Dallal.Documents.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Dallal.Documents;

[ApiExplorerSettings(GroupName = "Broker,Admin")]
[Authorize(Roles = "Broker,Admin")]
public class DocumentsAppService(IAmazonS3 s3Client, IConfiguration configuration)
    : DallalAppService,
        IDocumentsAppService,
        ITransientDependency
{
    public async Task<PresignedUrlDto> GetPresignedUrl(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
        }

        var containerName = configuration["S3:ContainerName"];
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new AbpException("S3 container name is not configured");
        }

        // Generate a unique file key to avoid conflicts
        var guid = Guid.NewGuid();
        var fileKey = $"documents/{guid}/{fileName}";
        var guidFileName = $"{guid}_{fileName}";

        // Create the presigned URL request
        var request = new GetPreSignedUrlRequest
        {
            BucketName = containerName,
            Key = fileKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(60), // URL expires in 60 minutes
            ContentType = GetContentType(fileName),
        };

        // Generate and return the presigned URL
        var presignedUrl = await s3Client.GetPreSignedURLAsync(request);

        return new PresignedUrlDto { Url = presignedUrl, FileName = guidFileName };
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
