using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.ThirdParty;

namespace Dallal_Backend_v2.Helpers.EntityDtoMappers;

public static class BrokerMapper
{
    public static async Task<BrokerDto> GetDtoFromSubmission(
        User user,
        Submission? submission,
        S3 s3Service
    )
    {
        var s3Url = await s3Service.CreateDocumentDto(user.ProfileImage);

        var documents = (
            submission?.GetExpectedValue<List<Document>>(nameof(Broker.Documents))
            ?? user.Broker!.Documents
            ?? []
        );
        var documentDtos = (
            await Task.WhenAll(documents.Select(s3Service.CreateDocumentDto))
        ).ToList();
        var userDto = new BrokerDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImage = s3Url,
            Phone = user.Phone,
            AgencyName =
                submission?.GetExpectedValue<string>(nameof(Broker.AgencyName))
                ?? user.Broker!.AgencyName,
            CertificateNumber =
                submission?.GetExpectedValue<string>(nameof(Broker.CertificateNumber))
                ?? user.Broker!.CertificateNumber,
            Description =
                submission?.GetExpectedValue<string>(nameof(Broker.Description))
                ?? user.Broker!.Description,
            Documents = documentDtos!,
            Status =
                submission?.GetExpectedValue<BrokerStatus>(nameof(Broker.Status))
                ?? user.Broker!.Status,
        };
        return userDto;
    }
}
