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

            Buyer? buyer = await _context.Buyers.SingleOrDefaultAsync(x => x.Email == email);

            if (buyer is null)
            {
                image ??= "https://picsum.photos/500";
                givenName ??= "";
                familyName ??= "";
                string fullName;

                if (
                    !string.IsNullOrEmpty((string)givenName)
                    && !string.IsNullOrEmpty((string)familyName)
                )
                {
                    fullName = $"{givenName} {familyName}";
                }
                else
                {
                    fullName = email;
                }

                buyer = new Buyer
                {
                    Id = new Guid(),
                    Email = email,
                    Name = fullName,
                    ProfileImage = (string)image,
                    Password = "oAuth User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedAt = null,
                };
                await _context.Buyers.AddAsync(buyer);
                await _context.SaveChangesAsync();
            }

            return CreateToken(buyer);
        }
        catch (FirebaseAuthException e)
        {
            throw new ArgumentException();
        }
    }

    [HttpPost("buyer/login")]
    [ProducesResponseType(typeof(AuthenticatedUser), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticatedUser> Login([FromBody] LoginRequest request)
    {
        Buyer? buyer = await _context.Buyers.SingleOrDefaultAsync(buyer =>
            buyer.Email == request.Email
        );
        if (buyer == null || BCrypt.Net.BCrypt.Verify(request.Password, buyer.Password))
        {
            throw new UnauthorizedAccessException("Invalid Email or Password");
        }

        return CreateToken(buyer);
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
            },
        };
        return response;
    }
}
