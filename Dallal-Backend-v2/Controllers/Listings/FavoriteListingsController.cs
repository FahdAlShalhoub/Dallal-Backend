using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("listings/favorites")]
[Authorize(Roles = "Buyer")]
public class FavoriteListingsController(DatabaseContext _context, S3 s3) : DallalController
{
    [HttpGet]
    public async Task<PaginatedList<ListingDto>> GetFavoriteListings(
        int pageNumber = 1,
        int pageSize = 10
    )
    {
        var query = _context.Listings.Where(i => i.Favorites.Any(f => f.Id == UserId));

        var listingsQuery = await query
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Definition)
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Option)
            .OrderByDescending(i => i.CreatedAt)
            .Select(ListingMapper.SelectToQueryDto(UserIdOrNull))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var listings = await Task.WhenAll(
            listingsQuery.Select(async listing => await ListingMapper.SelectToDto(listing, s3))
        );
        var count = await query.CountAsync();

        return new PaginatedList<ListingDto>([.. listings], pageNumber, count, pageSize);
    }

    [HttpPost()]
    public async Task AddFavoriteListing([FromBody] List<Guid> newFavorites)
    {
        var userId = UserId;
        var buyer =
            await _context
                .Buyers.Include(b => b.FavoriteListings)
                .FirstOrDefaultAsync(b => b.Id == userId)
            ?? throw new EntityNotFoundException(typeof(Buyer), userId);

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
        var userId = UserId;
        var buyer =
            await _context
                .Buyers.Include(b => b.FavoriteListings)
                .FirstOrDefaultAsync(b => b.Id == userId)
            ?? throw new EntityNotFoundException(typeof(Buyer), userId);

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
