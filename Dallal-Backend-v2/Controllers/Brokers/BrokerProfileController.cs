using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Brokers;

[Route("[controller]")]
[Authorize()]
public class BrokerProfileController(
    DatabaseContext _context,
    OtpService _otpService,
    SubmissionService _submissionService
) : DallalController
{
    [HttpGet()]
    public async Task<BrokerDto> GetProfile()
    {
        var userId = UserId;
        var userDto = await _context
            .Users.Where(u => u.Id == userId && u.Broker != null)
            .Select(BrokerMapper.SelectUserToBrokerDto())
            .FirstOrDefaultAsync();

        if (userDto == null)
            throw new EntityNotFoundException(typeof(Broker), userId);

        return userDto;
    }

    [HttpPost("otp")]
    public async Task<string> SendOtp(string phoneNumber)
    {
        var verificationSid = await _otpService.GenerateOtp(phoneNumber);
        return verificationSid;
    }

    [HttpPost("otp/verify")]
    public async Task VerifyOtp(string phoneNumber, string otp, string verificationSid)
    {
        await _otpService.VerifyOtp(phoneNumber, otp, verificationSid);
        var userId = UserId;
        var user = await _context.Users.Include(i => i.Broker).FirstAsync(i => i.Id == userId);

        user.Phone = phoneNumber;
        if (user.Broker == null)
            user.AddBroker(new Broker(userId) { Status = BrokerStatus.Pending });

        if (user.Broker!.Status is BrokerStatus.MissingData or BrokerStatus.Pending)
        {
            await _submissionService.UpsertSubmission(
                SubmissionType.BrokerAccount,
                userId,
                user.Broker,
                new Broker(userId)
                {
                    Status = BrokerStatus.Approved,
                    AgencyAddress = user.Broker.AgencyAddress,
                    AgencyName = user.Broker.AgencyName,
                    AgencyPhone = user.Broker.AgencyPhone,
                    AgencyEmail = user.Broker.AgencyEmail,
                    AgencyWebsite = user.Broker.AgencyWebsite,
                    AgencyLogo = user.Broker.AgencyLogo,
                }
            );
        }
        await _context.SaveChangesAsync();
    }
}
