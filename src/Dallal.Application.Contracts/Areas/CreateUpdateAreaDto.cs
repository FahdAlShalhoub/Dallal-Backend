using System;
using System.ComponentModel.DataAnnotations;
using Dallal.Localization;

namespace Dallal.Areas;

public class CreateUpdateAreaDto
{
    public Guid? ParentId { get; set; }

    [Required]
    public LocalizedStringDto Name { get; set; } = default!;
}
