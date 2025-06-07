using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
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
                request.UserType
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
        UserType userType
    )
    {
        if (userType == UserType.Buyer)
        {
            Buyer? buyer = await _context.Buyers.SingleOrDefaultAsync(x => x.Email == email);
            if (buyer is null)
            {
                buyer = new Buyer
                {
                    Id = new Guid(),
                    Email = email,
                    Name = $"{givenName as string ?? ""} {familyName as string ?? ""}",
                    ProfileImage = image as string,
                    Password = "oAuth User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedAt = null,
                };
                await _context.Buyers.AddAsync(buyer);
                await _context.SaveChangesAsync();
            }
            return buyer;
        }
        else if (userType == UserType.Broker)
        {
            Broker? broker = await _context.Brokers.SingleOrDefaultAsync(x => x.Email == email);
            if (broker is null)
            {
                broker = new Broker
                {
                    Id = new Guid(),
                    Email = email,
                    Status = BrokerStatus.Pending,
                    Name = $"{givenName as string ?? ""} {familyName as string ?? ""}",
                    ProfileImage = image as string,
                    Password = "oAuth User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedAt = null,
                };
                await _context.Brokers.AddAsync(broker);
                await _context.SaveChangesAsync();
            }
            return broker;
        }
        else if (userType == UserType.Admin)
        {
            Admin? admin = await _context.Admins.SingleOrDefaultAsync(x => x.Email == email);
            return admin ?? throw new UnauthorizedAccessException("Invalid User Type");
        }
        throw new UnauthorizedAccessException("Invalid User Type");
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

    private AuthenticatedUser CreateToken(BaseUser user)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        UserType userType = user switch
        {
            Buyer => UserType.Buyer,
            Broker => UserType.Broker,
            Admin => UserType.Admin,
            _ => throw new Exception("Invalid User Type"),
        };

        claims.Add(new Claim(ClaimTypes.Role, userType.ToString()));

        var response = new AuthenticatedUser
        {
            AccessToken = _jwtService.GenerateToken(claims),
            User = new UserInfo
            {
                Image = user.ProfileImage,
                Name = user.Name,
                Email = user.Email,
                Type = userType,
                Phone = user.Phone,
                PreferredLanguage = user.PreferredLanguage,
            },
        };
        return response;
    }
}
