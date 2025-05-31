using System;
using Volo.Abp.Application.Dtos;

namespace Dallal.Listings.Dtos;

public class GetListingListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? BrokerId { get; set; }
}
