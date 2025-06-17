using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Brokers.Dtos;

public class BrokerDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImage { get; set; }
    public BrokerStatus Status { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyAddress { get; set; }
    public string? AgencyPhone { get; set; }
    public string? AgencyEmail { get; set; }
    public string? AgencyWebsite { get; set; }
    public string? AgencyLogo { get; set; }
}
