using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
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

    public AuthController(FirebaseTokenVerifier firebaseTokenVerifier, JwtService jwtService, DatabaseContext context)
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
            var email = (string) firebaseToken.Claims.GetValueOrDefault("email")!;
            firebaseToken.Claims.TryGetValue("picture", out object? image);

            image ??= "https://picsum.photos/500";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var response = new AuthenticatedUser
            {
                AccessToken = _jwtService.GenerateToken(claims),
                User = new UserInfo
                {
                    Image = (string) image,
                    Name = "Bob",
                    Email = email
                }
            };
            return response;
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
        Buyer? buyer = await _context.Buyers.SingleOrDefaultAsync(buyer => buyer.Email == request.Email);
        if (buyer == null || BCrypt.Net.BCrypt.Verify(request.Password, buyer.Password))
        {
            throw new UnauthorizedAccessException("Invalid Email or Password");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var response = new AuthenticatedUser
        {
            AccessToken = _jwtService.GenerateToken(claims),
            User = new UserInfo
            {
                Image = "https://picsum.photos/100/100",
                Name = buyer.Name,
                Email = request.Email
            }
        };
        return response;
    }
}