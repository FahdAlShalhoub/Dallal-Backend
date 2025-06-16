using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Dallal_Backend_v2.Services;

public class JwtService(string jwtSecretKey, string issuer)
{
    private readonly SymmetricSecurityKey _jwtSecretKey = new(Encoding.UTF8.GetBytes(jwtSecretKey));

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        return new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: new SigningCredentials(
                    _jwtSecretKey,
                    SecurityAlgorithms.HmacSha256
                )
            )
        );
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            IssuerSigningKey = _jwtSecretKey,
            RoleClaimType = ClaimTypes.Role,
        };
    }
}
