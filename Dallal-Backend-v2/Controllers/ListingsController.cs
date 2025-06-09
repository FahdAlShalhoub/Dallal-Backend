using Dallal_Backend_v2.Controllers.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("listings")]
public class ListingsController : DallalController
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
    [ProducesResponseType(typeof(PaginatedList<ListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Listings(int pageIndex, int pageSize)
    {
        var listings = await _context
            .Listings
            .Include(listing => listing.Broker)
            .Include(listing => listing.Area)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Select(listing => new ListingDto
            {
                Id = listing.Id,
                Name = listing.Name,
                Description = listing.Description,
                Broker = new BrokerDto
                {
                    Id = listing.Broker.Id,
                    Email = listing.Broker.Email,
                    Name = listing.Broker.Name,
                },
                Area = listing.Area.Name,
                Currency = listing.Currency,
                PricePerContract = listing.PricePerContract,
                BedroomCount = listing.BedroomCount,
                BathroomCount = listing.BathroomCount,
                AreaInMetersSq = listing.AreaInMetersSq,
                ListingType = listing.ListingType,
                PropertyType = listing.PropertyType,
                RentalContractPeriod = listing.RentalContractPeriod,
                Details = null,
                PricePerYear = listing.PricePerYear,
                CreatedAt = listing.CreatedAt
            })
            .ToListAsync();

        var count = await _context.Listings.CountAsync();
        var totalPages = (int) Math.Ceiling(count / (double) pageSize);

        return Ok(new PaginatedList<ListingDto>(listings, pageIndex, totalPages));
    }
}