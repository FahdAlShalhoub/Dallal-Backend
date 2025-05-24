using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Dallal.Otps.Events;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Dallal.Otps;

public class Otp : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string EncryptedOtp { get; set; } = default!;

    public string Reference { get; set; } = default!;
    public OtpReasonEnum Reason { get; set; }

    public int RemainingAttempts { get; set; } = 3;
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public OtpStatusEnum Status { get; set; } = OtpStatusEnum.NotUsed;

    public string? MobileNumber { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public string Locale { get; set; } = default!;

    private Otp() { }

    public Otp(
        Guid id,
        OtpReasonEnum reason,
        string reference,
        string otp,
        string? mobileNumber,
        string? email
    )
        : base(id)
    {
        Reason = reason;
        Reference = reference;
        EncryptedOtp = GetEncryptedOtp(otp);
        TenantId = AsyncLocalCurrentTenantAccessor.Instance.Current?.TenantId;
        Locale = Thread.CurrentThread.CurrentUICulture.Name;
        MobileNumber = mobileNumber;
        Email = email;
        if (string.IsNullOrWhiteSpace(mobileNumber) && string.IsNullOrWhiteSpace(email))
            throw new AbpValidationException(
                [
                    new ValidationResult(
                        "MobileNumber or Email must be provided",
                        [nameof(MobileNumber), nameof(Email)]
                    ),
                ]
            );
        AddDistributedEvent(
            new SendOtpMessage
            {
                Id = id,
                Otp = otp,
                TenantId = TenantId,
            }
        );
    }

    private string GetEncryptedOtp(string otp) =>
        // Convert.ToBase64String(
        //     SHA256.HashData(
        //         Encoding.UTF8.GetBytes($"{otp}{Reference.GetHashCode() ^ Id.GetHashCode()}")
        //     )
        // );
        otp;

    public bool IsValid(string otp)
    {
        if (Status != OtpStatusEnum.NotUsed)
            return false;

        if (RemainingAttempts <= 0)
        {
            Status = OtpStatusEnum.AttemptsExceeded;
            return false;
        }

        if (DateTime.UtcNow > ExpirationDate)
        {
            Status = OtpStatusEnum.Expired;
            return false;
        }

        if (GetEncryptedOtp(otp) == EncryptedOtp)
        {
            Status = OtpStatusEnum.Used;
            return true;
        }

        RemainingAttempts--;
        return false;
    }
}
