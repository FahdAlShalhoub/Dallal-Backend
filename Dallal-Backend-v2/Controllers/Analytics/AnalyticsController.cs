using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("analytics")]
public class AnalyticsController(
    ListingViewService _listingViewService
) : DallalController
{
    [HttpPost("listings/{id:guid}/view")]
    public async Task<IActionResult> AddView(
        [FromRoute] Guid id,
        [FromBody] AddViewDto? viewData = null
    )
    {
        var userId = UserIdOrNull;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        try
        {
            if (userId != null)
            {
                // Authenticated view
                await _listingViewService.AddViewAsync(id, userId, null, ipAddress, userAgent);
            }
            else
            {
                // Anonymous view - require DeviceUuid
                if (string.IsNullOrEmpty(viewData?.DeviceUuid))
                {
                    return BadRequest("DeviceUuid is required for anonymous views");
                }

                await _listingViewService.AddViewAsync(
                    id,
                    null,
                    viewData.DeviceUuid,
                    ipAddress,
                    userAgent
                );
            }

            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("listings/{id:guid}/view-count")]
    public async Task<ViewCountResponse> GetViewCount([FromRoute] Guid id)
    {
        var totalViews = await _listingViewService.GetViewCountAsync(id);
        var uniqueViews = await _listingViewService.GetUniqueViewCountAsync(id);

        return new ViewCountResponse { TotalViews = totalViews, UniqueViews = uniqueViews };
    }
}

public class AddViewDto
{
    public string? DeviceUuid { get; set; }
}

public class ViewCountResponse
{
    public int TotalViews { get; set; }
    public int UniqueViews { get; set; }
}