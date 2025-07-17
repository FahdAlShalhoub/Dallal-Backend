using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Entities.Listings;

[Table("ListingViews")]
public class ListingView : BaseEntity
{

    [Required]
    public Guid ListingId { get; set; }

    [ForeignKey(nameof(ListingId))]
    public virtual Listing Listing { get; set; } = null!;

    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    public string? DeviceUuid { get; set; }

    [Required]
    public DateTime ViewedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}