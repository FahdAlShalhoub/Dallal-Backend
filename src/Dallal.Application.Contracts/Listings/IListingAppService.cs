using System;
using Dallal.Listings.Dtos;
using Volo.Abp.Application.Services;

namespace Dallal.Listings;

public interface IListingAppService
    : ICrudAppService<
        ListingDto,
        Guid,
        GetListingListInput,
        CreateUpdateListingDto,
        CreateUpdateListingDto
    > { }
