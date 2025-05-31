using System;
using System.ComponentModel.DataAnnotations;
using Dallal.Localization.Dtos;

namespace Dallal.Areas.Dtos;

public class CreateUpdateAreaDto
{
    public Guid? ParentId { get; set; }

    [Required]
    public LocalizedStringDto Name { get; set; } = default!;
}
