using Dallal_Backend_v2.Controllers.Profiles.Dtos;

namespace Dallal_Backend_v2.Controllers.Brokers.Dtos;

public class UpdateBrokerInfoRequest : UpdateProfileProfileRequest
{
    public string? AgencyName { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Description { get; set; }
}
