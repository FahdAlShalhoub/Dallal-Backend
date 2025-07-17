using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Repositories.Brokers;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Brokers;

[Route("[controller]")]
public class BrokerController(IBrokerRepository _brokerRepository, S3 _s3Service) : DallalController
{
    [HttpGet("approved")]
    public async Task<PaginatedList<BrokerDto>> GetApprovedBrokers(
        int pageNumber = 1,
        int pageSize = 10
    )
    {
        var result = await _brokerRepository.GetPaginatedAsync(
            pageNumber,
            pageSize,
            i => i.Status == BrokerStatus.Approved,
            include: i => i.Include(i => i.User)
        );

        var brokerDtos = await BrokerMapper.MapToDtoListAsync(result.Items, _s3Service);

        return new PaginatedList<BrokerDto>(brokerDtos, pageNumber, (int)result.Count, pageSize);
    }
}
