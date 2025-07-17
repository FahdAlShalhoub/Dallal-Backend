using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.ThirdParty;

namespace Dallal_Backend_v2.Helpers.EntityDtoMappers;

public static class BrokerMapper
{
    public static async Task<BrokerDto> GetDtoFromSubmission(
        User user,
        Broker? submissionBroker,
        S3 s3Service
    )
    {
        var s3Url = await s3Service.CreateDocumentDto(user.ProfileImage);
        var documents = submissionBroker?.Documents ?? user.Broker!.Documents ?? [];
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
            AgencyName = submissionBroker?.AgencyName ?? user.Broker!.AgencyName,
            CertificateNumber =
                submissionBroker?.CertificateNumber ?? user.Broker!.CertificateNumber,
            Description = submissionBroker?.Description ?? user.Broker!.Description,
            Documents = documentDtos!,
            Status = submissionBroker?.Status ?? user.Broker!.Status,
        };
        return userDto;
    }

    public static async Task<BrokerDto> MapToDtoAsync(Broker broker, S3 s3Service)
    {
        var user = broker.User;
        var profileImageDto = await s3Service.CreateDocumentDto(user.ProfileImage);

        return new BrokerDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImage = profileImageDto,
            Status = broker.Status,
            AgencyName = broker.AgencyName,
            CertificateNumber = broker.CertificateNumber,
            Description = broker.Description,
            Documents =
                broker
                    .Documents?.Select(doc => new DocumentDto
                    {
                        FileName = doc.FileName,
                        NameInBucket = doc.NameInBucket,
                        PlaceHolderNameInBucket = doc.PlaceHolderNameInBucket,
                    })
                    .ToList() ?? new List<DocumentDto>(),
        };
    }

    public static async Task<List<BrokerDto>> MapToDtoListAsync(
        List<Broker> brokers,
        S3 s3Service
    ) => [.. await Task.WhenAll(brokers.Select(broker => MapToDtoAsync(broker, s3Service)))];
}
