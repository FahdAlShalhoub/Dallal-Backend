using System;
using System.Threading.Tasks;

namespace Dallal.Otps;

public interface IOtpAppService
{
    Task<Guid> CreateAsync(
        OtpReasonEnum reason,
        string? mobileNumber,
        string? email,
        string? reference = null
    );
}
