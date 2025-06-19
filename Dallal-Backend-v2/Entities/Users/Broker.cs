using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Helpers;

namespace Dallal_Backend_v2.Entities.Users;

public class Broker
{
    private Broker() { }

    public Broker(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }

    [DoNotIncludeInSubmission]
    public User User { get; set; }
    public BrokerStatus Status { get; set; }
    public string? AgencyName { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public List<Document>? Documents { get; set; }

    public bool IsMinimumInfoSet() => User.Phone != null && User.Email != null;
}
