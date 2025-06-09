using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
public abstract class DallalController : ControllerBase
{
    protected Guid UserId =>
        Guid.Parse(
            User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier
            )?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims.")
        );
}
