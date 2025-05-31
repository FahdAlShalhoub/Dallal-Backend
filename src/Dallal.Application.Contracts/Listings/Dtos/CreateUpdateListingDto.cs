using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dallal.Listings.Dtos;

public class CreateUpdateListingDto
{
    public Guid? BrokerId { get; set; }

    [Required]
    [StringLength(maximumLength: 256)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(maximumLength: 2000)]
    public string Description { get; set; } = default!;

    [Required]
    public Guid AreaId { get; set; }

    public List<CreateUpdateListingDetailDto> Details { get; set; } = [];
}
