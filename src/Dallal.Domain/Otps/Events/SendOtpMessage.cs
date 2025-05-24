using System;
using Volo.Abp.MultiTenancy;

namespace Dallal.Otps.Events;

public class SendOtpMessage : IMultiTenant
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Otp { get; set; } = default!;
}
