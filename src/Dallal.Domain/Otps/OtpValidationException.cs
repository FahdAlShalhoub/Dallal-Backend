using System;
using Volo.Abp;

namespace Dallal.Otps;

[Serializable]
public class OtpValidationException(OtpStatusEnum status)
    : UserFriendlyException(GetErrorCode(status))
{
    private static string GetErrorCode(OtpStatusEnum status) => $"dallal:otp.validation.{status}";
}
