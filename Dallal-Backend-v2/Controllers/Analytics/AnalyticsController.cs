using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Controllers.Analytics.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("analytics")]
public class AnalyticsController(ListingViewService _listingViewService) : DallalController
{
    [HttpPost("listings/{id:guid}/view")]
    public async Task AddView([FromRoute] Guid id, [FromBody] AddViewDto? viewData = null)
    {
        var userId = UserIdOrNull;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        if (userId != null)
        {
            // Authenticated view
            await _listingViewService.AddViewAsync(id, userId, null, ipAddress, userAgent);
        }
        else
        {
            // Anonymous view - require DeviceUuid
            if (string.IsNullOrEmpty(viewData?.DeviceUuid))
                throw new ValidationException("DeviceUuid is required for anonymous views");

            await _listingViewService.AddViewAsync(
                id,
                null,
                viewData.DeviceUuid,
                ipAddress,
                userAgent
            );
        }
    }
}
