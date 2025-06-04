using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;


[ApiController]
[Route("listings")]
public class ListingsController : ControllerBase
{
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentListings()
    {
        return Ok();
    }

}