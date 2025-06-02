using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly FirebaseTokenVerifier _firebaseTokenVerifier;
    private readonly JwtService _jwtService;

    public AuthController(FirebaseTokenVerifier firebaseTokenVerifier, JwtService jwtService)
    {
        _firebaseTokenVerifier = firebaseTokenVerifier;
        _jwtService = jwtService;
    }

    [HttpPost("oauth")]
    public async Task<IActionResult> OAuth([FromBody] OAuthRequest request)
    {
        try
        {
            var firebaseToken = await _firebaseTokenVerifier.VerifyIdTokenAsync(request.idToken);
            var email = (string) firebaseToken.Claims.GetValueOrDefault("email")!;

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
                    Name = "Bob",
                    Email = email
                }
            };
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
