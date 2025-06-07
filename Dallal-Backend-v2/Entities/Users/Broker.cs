using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Entities.Users;

public class Broker : BaseUser
{
    public BrokerStatus Status { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyAddress { get; set; }
    public string? AgencyPhone { get; set; }
    public string? AgencyEmail { get; set; }
    public string? AgencyWebsite { get; set; }
    public string? AgencyLogo { get; set; }
}
