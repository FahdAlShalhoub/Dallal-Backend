using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dallal.Otps;

[ApiExplorerSettings(GroupName = "Broker,Customer,Admin")]
public class OtpAppService(OtpManager _otpManger) : DallalAppService, IOtpAppService
{
    public async Task<Guid> CreateAsync(
        OtpReasonEnum reason,
        string? mobileNumber,
        string? email,
        string? reference = null
    )
    {
        return await _otpManger.CreateOtpAsync(
            reason,
            reference ?? mobileNumber ?? email!,
            mobileNumber,
            email
        );
    }
}
