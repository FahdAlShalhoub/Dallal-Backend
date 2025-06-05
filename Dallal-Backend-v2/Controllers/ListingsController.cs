using Dallal_Backend_v2.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;


[ApiController]
[Route("listings")]
public class ListingsController : ControllerBase
{
    private readonly DatabaseContext _context;
    public ListingsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentListings()
    {
        return Ok();
    }

    [HttpGet(Name = "GetListings")]
    [ProducesResponseType(typeof(IEnumerable<Listing>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Listings()
    {
        var listings = await _context.Listings.ToListAsync();
        return Ok(listings);
    }

}