using System;
using System.Linq;
using System.Threading.Tasks;
using Dallal.Listings.Dtos;
using Dallal.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Dallal.Listings;

[ApiExplorerSettings(GroupName = "Admin")]
[Authorize(DallalPermissions.Listings.Default)]
public class ListingAppService
    : CrudAppService<
        Listing,
        ListingDto,
        Guid,
        GetListingListInput,
        CreateUpdateListingDto,
        CreateUpdateListingDto
    >,
        IListingAppService
{
    public ListingAppService(IRepository<Listing, Guid> repository)
        : base(repository)
    {
        CreatePolicyName = DallalPermissions.Listings.Create;
        UpdatePolicyName = DallalPermissions.Listings.Update;
        DeletePolicyName = DallalPermissions.Listings.Delete;
        GetPolicyName = DallalPermissions.Listings.Default;
        GetListPolicyName = DallalPermissions.Listings.Default;
    }

    protected override async Task<IQueryable<Listing>> CreateFilteredQueryAsync(
        GetListingListInput input
    )
    {
        var query = await Repository.GetQueryableAsync();

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Name),
            x => x.Name.Contains(input.Name!)
        );

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Description),
            x => x.Description.Contains(input.Description!)
        );

        query = query.WhereIf(input.AreaId.HasValue, x => x.AreaId == input.AreaId!.Value);
        query = query.WhereIf(input.BrokerId.HasValue, x => x.BrokerId == input.BrokerId!.Value);

        return query;
    }

    protected override Task<Listing> MapToEntityAsync(CreateUpdateListingDto createInput) =>
        Task.FromResult<Listing>(
            new()
            {
                Name = createInput.Name,
                Description = createInput.Description,
                AreaId = createInput.AreaId,
                BrokerId = CurrentUser.Id,
            }
        );

    protected override async Task MapToEntityAsync(
        CreateUpdateListingDto updateInput,
        Listing entity
    )
    {
        entity.Name = updateInput.Name;
        entity.Description = updateInput.Description;
        entity.AreaId = updateInput.AreaId;
        entity.BrokerId = CurrentUser.Id;
    }
}
