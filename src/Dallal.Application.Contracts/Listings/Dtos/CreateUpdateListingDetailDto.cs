using System;
using System.ComponentModel.DataAnnotations;

namespace Dallal.Listings.Dtos;

public class CreateUpdateListingDetailDto
{
    [Required]
    public Guid DefinitionId { get; set; }

    [Required]
    public Guid OptionId { get; set; }
}
