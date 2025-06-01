using System;
using System.Linq;
using System.Threading.Tasks;
using Dallal.Listings.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Dallal.Listings;

[ApiExplorerSettings(GroupName = "Customer,Broker")]
public class ListingReadOnlyAppService
    : ReadOnlyAppService<Listing, ListingDto, Guid, GetListingListInput>,
        IListingReadOnlyAppService
{
    public ListingReadOnlyAppService(IRepository<Listing, Guid> repository)
        : base(repository) { }

    protected override async Task<IQueryable<Listing>> CreateFilteredQueryAsync(
        GetListingListInput input
    )
    {
        var query = await Repository.WithDetailsAsync(x => x.Area);

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Name),
            x => x.Name.Contains(input.Name!)
        );

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Description),
            x => x.Description.Contains(input.Description!)
        );

        query = query.WhereIf(input.AreaId.HasValue, x => x.AreaId == input.AreaId.Value);

        query = query.WhereIf(input.BrokerId.HasValue, x => x.BrokerId == input.BrokerId.Value);

        return query;
    }
}
