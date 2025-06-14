using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public abstract class DallalController : ControllerBase
{
    protected Guid? UserIdOrNull =>
        User
            .Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier
            )
            ?.Value
            is string userIdString
        && Guid.TryParse(userIdString, out var userId)
            ? userId
            : null;
    protected Guid UserId =>
        UserIdOrNull ?? throw new UnauthorizedAccessException("User is not authenticated");
}
