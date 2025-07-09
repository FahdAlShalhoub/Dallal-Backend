using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Brokers;

[Route("[controller]")]
public class BrokerController(DatabaseContext _context, S3 _s3Service) : DallalController
{
    [HttpGet("approved")]
    public async Task<PaginatedList<BrokerDto>> GetApprovedBrokers(
        int pageNumber = 1,
        int pageSize = 10
    )
    {
        var query = _context
            .Users.Where(u => u.Broker != null && u.Broker.Status == BrokerStatus.Approved)
            .Include(u => u.Broker)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName);

        var totalCount = await query.CountAsync();

        var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var brokerDtos = new List<BrokerDto>();

        foreach (var user in users)
        {
            var profileImageDto = await _s3Service.CreateDocumentDto(user.ProfileImage);

            brokerDtos.Add(
                new BrokerDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfileImage = profileImageDto,
                    Status = user.Broker!.Status,
                    AgencyName = user.Broker.AgencyName,
                    CertificateNumber = user.Broker.CertificateNumber,
                    Description = user.Broker.Description,
                    Documents =
                        user.Broker.Documents?.Select(doc => new DocumentDto
                            {
                                FileName = doc.FileName,
                                NameInBucket = doc.NameInBucket,
                                PlaceHolderNameInBucket = doc.PlaceHolderNameInBucket,
                            })
                            .ToList() ?? new List<DocumentDto>(),
                }
            );
        }

        return new PaginatedList<BrokerDto>(brokerDtos, pageNumber, totalCount, pageSize);
    }
}
