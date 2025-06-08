using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : DallalController
{
    private readonly FirebaseTokenVerifier _firebaseTokenVerifier;
    private readonly JwtService _jwtService;
    private readonly DatabaseContext _context;

    public AuthController(
        FirebaseTokenVerifier firebaseTokenVerifier,
        JwtService jwtService,
        DatabaseContext context
    )
    {
        _firebaseTokenVerifier = firebaseTokenVerifier;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("oauth")]
    public async Task<AuthenticatedUser> OAuth([FromBody] OAuthRequest request)
    {
        try
        {
            var firebaseToken = await _firebaseTokenVerifier.VerifyIdTokenAsync(request.idToken);
            var emailClaim = firebaseToken.Claims.GetValueOrDefault("email");

            if (emailClaim is not string email)
            {
                throw new Exception("Invalid Email Claim From Firebase Token");
            }

            firebaseToken.Claims.TryGetValue("picture", out object? image);
            firebaseToken.Claims.TryGetValue("given_name", out object? givenName);
            firebaseToken.Claims.TryGetValue("family_name", out object? familyName);
            BaseUser user = await GetOrCreateUser(
                email,
                image,
                givenName,
                familyName,
                request.UserType,
                request.PreferredLanguage
            );

            return CreateToken(user);
        }
        catch (FirebaseAuthException)
        {
            throw new ArgumentException();
        }
    }

    private async Task<BaseUser> GetOrCreateUser(
        string email,
        object? image,
        object? givenName,
        object? familyName,
        UserType userType,
        string preferredLanguage
    )
    {
        switch (userType)
        {
            case UserType.Buyer:
            {
                Buyer? buyer = await _context.Buyers.SingleOrDefaultAsync(x => x.Email == email);
                if (buyer is not null)
                {
                    return buyer;
                }

                var fullName = $"{givenName as string ?? ""} {familyName as string ?? ""}";

                if (givenName == null && familyName == null)
                {
                    fullName = email;
                }

                buyer = new Buyer
                {
                    Id = new Guid(),
                    Email = email,
                    Name = fullName,
                    ProfileImage = image as string,
                    Password = "oAuth User",
                    PreferredLanguage = preferredLanguage,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedAt = null,
                };
                await _context.Buyers.AddAsync(buyer);
                await _context.SaveChangesAsync();

                return buyer;
            }
            case UserType.Broker:
            {
                Broker? broker = await _context.Brokers.SingleOrDefaultAsync(x => x.Email == email);
                if (broker is not null)
                {
                    return broker;
                }

                var fullName = $"{givenName as string ?? ""} {familyName as string ?? ""}";

                if (givenName == null && familyName == null)
                {
                    fullName = email;
                }

                broker = new Broker
                {
                    Id = new Guid(),
                    Email = email,
                    Status = BrokerStatus.Pending,
                    Name = fullName,
                    ProfileImage = image as string,
                    Password = "oAuth User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PreferredLanguage = preferredLanguage,
                    DeletedAt = null,
                };
                await _context.Brokers.AddAsync(broker);
                await _context.SaveChangesAsync();

                return broker;
            }
            case UserType.Admin:
            {
                Admin? admin = await _context.Admins.SingleOrDefaultAsync(x => x.Email == email);
                return admin ?? throw new UnauthorizedAccessException("Invalid Email");
            }
            default:
                throw new UnauthorizedAccessException("Invalid User Type");
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticatedUser), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticatedUser> Login([FromBody] LoginRequest request)
    {
        BaseUser? user = request.UserType switch
        {
            UserType.Buyer => await _context.Buyers.SingleOrDefaultAsync(buyer =>
                buyer.Email == request.Email
            ),
            UserType.Broker => await _context.Brokers.SingleOrDefaultAsync(broker =>
                broker.Email == request.Email
            ),
            UserType.Admin => await _context.Admins.SingleOrDefaultAsync(admin =>
                admin.Email == request.Email
            ),
            _ => throw new UnauthorizedAccessException("Invalid User Type"),
        };
        if (user == null || BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new UnauthorizedAccessException("Invalid Email or Password");

        return CreateToken(user);
    }

    [HttpPut("info")]
    [Authorize]
    public async Task<UserInfo> UpdateLanguage([FromBody] UpdateUserRequest request)
    {
        if (request.Language is null && request.Name is null)
        {
            throw new BadHttpRequestException("Must provide at least one property to update for the user info");
        }

        var user =
            await _context.Users.FirstAsync(i => i.Id == UserId)
            ?? throw new Exception("User not found");

        if (request.Language != null)
        {
            user.PreferredLanguage = request.Language;
        }

        if (request.Name != null)
        {
            user.Name = request.Name;
        }

        await _context.SaveChangesAsync();
        return GenerateUserInfoDto(user, UserType.Buyer);
    }

    private AuthenticatedUser CreateToken(BaseUser user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        UserType userType = user switch
        {
            Buyer => UserType.Buyer,
            Broker => UserType.Broker,
            Admin => UserType.Admin,
            _ => throw new Exception("Invalid User Type"),
        };

        claims.Add(new Claim(ClaimTypes.Role, userType.ToString()));

        return new AuthenticatedUser
        {
            AccessToken = _jwtService.GenerateToken(claims),
            User = GenerateUserInfoDto(user, userType)
        };
    }

    private static UserInfo GenerateUserInfoDto(BaseUser user, UserType userType)
    {
        return new UserInfo
        {
            Image = user.ProfileImage!,
            Name = user.Name!,
            Email = user.Email!,
            Type = userType,
            Phone = user.Phone,
            PreferredLanguage = user.PreferredLanguage
        };
    }
}

public record struct UpdateUserRequest(
    string? Name,
    string? Language
);