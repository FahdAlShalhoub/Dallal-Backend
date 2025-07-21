using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Helpers;

namespace Dallal_Backend_v2.Entities.Users;

public class Broker : BaseEntity
{
    private Broker() { }

    public Broker(Guid id)
    {
        Id = id;
    }

    [DoNotIncludeInSubmission]
    [JsonIgnore]
    public User User { get; set; } = default!;
    public BrokerStatus Status { get; set; }
    public string? AgencyName { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public List<Document>? Documents { get; set; }

    public bool IsMinimumInfoSet() => User.Phone != null && User.Email != null;
}
