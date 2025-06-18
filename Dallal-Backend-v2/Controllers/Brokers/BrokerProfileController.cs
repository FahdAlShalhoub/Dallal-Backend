using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Controllers.Submissions;
using Dallal_Backend_v2.Controllers.Submissions.Dtos;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Brokers;

[Route("[controller]")]
[Authorize()]
public class BrokerProfileController(
    DatabaseContext _context,
    OtpService _otpService,
    SubmissionService _submissionService,
    S3 _s3Service
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

    [HttpGet("submission")]
    public async Task<SubmissionDto?> GetSubmission()
    {
        var userId = UserId;
        var submission = await _context.Submissions.FirstOrDefaultAsync(s =>
            s.ReferenceId == userId
            && s.Type == SubmissionType.BrokerAccount
            && (s.Status == SubmissionStatus.Rejected)
        );
        if (submission == null)
            return null;

        return SubmissionMapper.SelectToDto().Compile()(submission);
    }

    [HttpPost("otp")]
    public async Task<OtpDto> SendOtp(string phoneNumber)
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

    [HttpPut("info")]
    [Authorize(Roles = "Broker")]
    public async Task<BrokerDto> UpdateInfo([FromBody] UpdateBrokerInfoRequest request)
    {
        var userId = UserId;
        var user = await _context.Users.Include(i => i.Broker).FirstAsync(i => i.Id == userId);
        if (user.Broker == null)
            throw new EntityNotFoundException(typeof(Broker), userId);

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.ProfileImage = request.Image;
        user.PreferredLanguage = Thread.CurrentThread.CurrentCulture.Name;

        user.UpdatedAt = DateTime.UtcNow;

        var newBroker = new Broker(userId)
        {
            AgencyName = request.AgencyName,
            AgencyAddress = request.AgencyAddress,
            AgencyPhone = request.AgencyPhone,
            AgencyEmail = request.AgencyEmail,
            AgencyWebsite = request.AgencyWebsite,
            AgencyLogo = request.AgencyLogo,
            User = user,
        };

        if (newBroker.IsMinimumInfoSet())
        {
            newBroker.Status = BrokerStatus.Approved;
            await _submissionService.UpsertSubmission(
                SubmissionType.BrokerAccount,
                userId,
                user.Broker,
                newBroker
            );
        }
        else
        {
            user.Broker.Status = BrokerStatus.Pending;
            user.AddBroker(newBroker);
        }

        await _context.SaveChangesAsync();
        return BrokerMapper.SelectUserToBrokerDto().Compile()(user);
    }

    [HttpPost("documents/upload-documents")]
    [Authorize(Roles = "Broker")]
    public async Task<PresignedUrlDto> UploadDocuments([FromBody] UploadDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException(
                "File name cannot be null or empty",
                nameof(request.FileName)
            );

        var presignedUrl = await _s3Service.GetPresignedUrl(request.FileName, "brokers/" + UserId);
        return presignedUrl;
    }
}
