using System.Security;
using Twilio;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2;
using Twilio.Rest.Verify.V2.Service;

public class OtpService(IConfiguration configuration, IWebHostEnvironment _env)
{
    private readonly string _verifyServiceSid = configuration["Twilio:ServiceSid"]!;

    public async Task<string> GenerateOtp(string mobileNumber)
    {
        if (_env.IsDevelopment())
            return "dev";
        var result = await VerificationResource.CreateAsync(
            new CreateVerificationOptions(_verifyServiceSid, mobileNumber, "sms")
            {
                Locale = Thread.CurrentThread.CurrentUICulture.Name,
                // CustomFriendlyName = //not supported for some reason
                //     Thread.CurrentThread.CurrentUICulture.Name == "ar" ? "دلّال" : "Dallal",
            }
        );
        return result.Sid;
    }

    public async Task VerifyOtp(string mobileNumber, string otp, string verificationSid)
    {
        if (_env.IsDevelopment())
        {
            if (otp != "111111")
                throw new VerificationException("failed");
            return;
        }
        var result = await VerificationCheckResource.CreateAsync(
            new CreateVerificationCheckOptions(_verifyServiceSid)
            {
                To = mobileNumber,
                Code = otp,
                VerificationSid = verificationSid,
            }
        );

        // `pending`, `approved`, `canceled`, `max_attempts_reached`, `deleted`, `failed` or `expired`.
        var isApproved = result.Status == "approved";
        if (!isApproved)
            throw new VerificationException(result.Status);
    }
}
