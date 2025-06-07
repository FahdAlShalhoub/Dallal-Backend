using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
public abstract class DallalController : ControllerBase
{
    protected Guid UserId => Guid.Parse(User.Claims.First(c => c.Type == "sub").Value);
}
