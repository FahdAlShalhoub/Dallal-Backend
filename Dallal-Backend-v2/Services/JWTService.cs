using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Dallal_Backend_v2.Services;

public class JwtService(string jwtSecretKey)
{
    private readonly SymmetricSecurityKey _jwtSecretKey = new(Encoding.UTF8.GetBytes(jwtSecretKey));

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "your-issuer",
            audience: "your-audience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: new SigningCredentials(_jwtSecretKey, SecurityAlgorithms.HmacSha256))
        );
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "your-issuer",
            ValidAudience = "your-audience",
            IssuerSigningKey = _jwtSecretKey
        };
    }
}