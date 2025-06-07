using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("details")]
public class DetailsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public DetailsController(DatabaseContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetDetails()
    {
        var details = await _context
            .LisitngDetails.Include(detail => detail.Listing)
            .ToListAsync();

        return Ok(
            details.Select(detail => new DetailDto
            {
                Id = detail.Id,
                ListingId = detail.ListingId,
                Description = detail.Description,
                Images = detail.Images.Select(image => new ImageDto
                {
                    Id = image.Id,
                    Url = image.Url,
                    Description = image.Description
                }).ToList()
            })
        );
    }
