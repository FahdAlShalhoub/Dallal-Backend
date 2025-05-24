using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace Dallal.Otps;

public class OtpManager(
    IRepository<Otp, Guid> _otpRepository,
    IUnitOfWorkManager _unitOfWorkManager
) : DomainService
{
    public async Task<Guid> CreateOtpAsync(
        OtpReasonEnum reason,
        string reference,
        string? mobileNumber,
        string? email
    )
    {
        var isProduction =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        int otpAsInt = isProduction ? RandomNumberGenerator.GetInt32(100_000, 1_000_000) : 111111;
        var otp = otpAsInt.ToString().PadLeft(6, '0');
        var otpEntity = new Otp(
            GuidGenerator.Create(),
            reason,
            reference,
            otp,
            mobileNumber,
            email
        );
        await _otpRepository.InsertAsync(otpEntity, true);
        return otpEntity.Id;
    }

    public async Task<Otp> ValidateAsync(Guid id, string otp, OtpReasonEnum reason)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var otpEntity = await _otpRepository.GetAsync(id);
        var isValid = otpEntity.IsValid(otp);
        await _otpRepository.UpdateAsync(otpEntity, true);
        await uow.CompleteAsync();

        if (!isValid)
            throw new OtpValidationException(otpEntity.Status);

        if (otpEntity.Reason != reason)
            throw new EntityNotFoundException(typeof(Otp), id);

        return otpEntity;
    }
}
