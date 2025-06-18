using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Controllers.Profiles.Dtos;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Profiles;

[Route("[controller]")]
[Authorize]
public class ProfileController(DatabaseContext _context, S3 _s3Service) : DallalController
{
    [HttpPut("update")]
    public async Task<UserInfoDto> UpdateProfile([FromBody] UpdateProfileProfileRequest request)
    {
        var userId = UserId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new EntityNotFoundException(typeof(User), userId);

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.ProfileImage = request.Image;
        user.PreferredLanguage = Thread.CurrentThread.CurrentCulture.Name;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new UserInfoDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Image = user.ProfileImage,
            Phone = user.Phone,
            PreferredLanguage = user.PreferredLanguage,
            Roles = user.Roles,
        };
    }

    [HttpPost("upload-image")]
    public async Task<PresignedUrlDto> UploadImage([FromBody] UploadDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException(
                "File name cannot be null or empty",
                nameof(request.FileName)
            );

        var presignedUrl = await _s3Service.GetPresignedUrl(
            request.FileName,
            "users/" + UserId + "/profile-image"
        );
        return presignedUrl;
    }
}
