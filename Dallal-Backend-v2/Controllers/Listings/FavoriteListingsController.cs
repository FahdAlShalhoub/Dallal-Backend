using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("listings/favorites")]
[Authorize(Roles = "Buyer")]
public class FavoriteListingsController(DatabaseContext _context) : DallalController
{
    [HttpGet]
    public async Task<PaginatedList<ListingDto>> GetFavoriteListings(
        int pageNumber = 1,
        int pageSize = 10
    )
    {
        var query = _context.Listings.Where(i => i.Favorites.Any(f => f.Id == UserId));

        var listings = await query
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Definition)
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Option)
            .Take(pageSize)
            .Skip((pageNumber - 1) * pageSize)
            .OrderByDescending(i => i.CreatedAt)
            .Select(ListingMapper.SelectToDto(UserIdOrNull))
            .ToListAsync();
        var count = await query.CountAsync();

        return new PaginatedList<ListingDto>(listings, pageNumber, count, pageSize);
    }

    [HttpPost()]
    public async Task AddFavoriteListing([FromBody] List<Guid> newFavorites)
    {
        var buyer =
            await _context
                .Buyers.Include(b => b.FavoriteListings)
                .FirstOrDefaultAsync(b => b.Id == UserId)
            ?? throw new EntityNotFoundException(typeof(Buyer), UserId);

        var favoriteListings = await _context
            .Listings.Where(l => newFavorites.Contains(l.Id))
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Definition)
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Option)
            .ToListAsync();

        foreach (var listing in favoriteListings)
        {
            if (!buyer.FavoriteListings.Contains(listing))
                buyer.FavoriteListings.Add(listing);
        }

        await _context.SaveChangesAsync();
    }

    [HttpDelete]
    public async Task RemoveFavoriteListing([FromBody] List<Guid> favoritesToRemove)
    {
        var buyer =
            await _context
                .Buyers.Include(b => b.FavoriteListings)
                .FirstOrDefaultAsync(b => b.Id == UserId)
            ?? throw new EntityNotFoundException(typeof(Buyer), UserId);

        var favoriteListings = await _context
            .Listings.Where(l => favoritesToRemove.Contains(l.Id))
            .ToListAsync();

        foreach (var listing in favoriteListings)
        {
            if (buyer.FavoriteListings.Contains(listing))
                buyer.FavoriteListings.Remove(listing);
        }

        await _context.SaveChangesAsync();
    }
}
