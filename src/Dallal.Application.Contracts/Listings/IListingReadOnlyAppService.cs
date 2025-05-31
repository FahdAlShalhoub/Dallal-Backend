using System;
using Dallal.Listings.Dtos;
using Volo.Abp.Application.Services;

namespace Dallal.Listings;

public interface IListingReadOnlyAppService
    : IReadOnlyAppService<ListingDto, Guid, GetListingListInput> { }
