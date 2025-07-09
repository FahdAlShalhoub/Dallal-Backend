using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Brokers.Dtos;

public class BrokerDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DocumentDto? ProfileImage { get; set; }
    public BrokerStatus Status { get; set; }
    public string? AgencyName { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Description { get; set; }
    public List<DocumentDto> Documents { get; set; } = new();
}
