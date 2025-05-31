using System;
using Volo.Abp.Application.Dtos;

namespace Dallal.Areas;

public class GetAreaListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }
}
