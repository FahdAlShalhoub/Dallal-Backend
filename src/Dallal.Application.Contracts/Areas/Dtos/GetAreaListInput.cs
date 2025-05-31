using System;
using Volo.Abp.Application.Dtos;

namespace Dallal.Areas.Dtos;

public class GetAreaListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }
}
