using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Buyers;

public class BuyerRepository : Repository<Buyer>, IBuyerRepository
{
    public BuyerRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Listing>> GetFavoriteListingsAsync(Guid buyerId, int pageNumber, int pageSize)
    {
        var buyer = await _dbSet
            .Include(b => b.FavoriteListings)
            .ThenInclude(l => l.Area)
            .Include(b => b.FavoriteListings)
            .ThenInclude(l => l.Broker)
            .ThenInclude(b => b!.User)
            .FirstOrDefaultAsync(b => b.Id == buyerId);

        if (buyer?.FavoriteListings == null)
            return Enumerable.Empty<Listing>();

        return buyer.FavoriteListings
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task AddFavoriteListingsAsync(Guid buyerId, List<Guid> listingIds)
    {
        var buyer = await _dbSet
            .Include(b => b.FavoriteListings)
            .FirstOrDefaultAsync(b => b.Id == buyerId);

        if (buyer != null)
        {
            var listings = await _context.Listings
                .Where(l => listingIds.Contains(l.Id))
                .ToListAsync();

            foreach (var listing in listings)
            {
                if (!buyer.FavoriteListings!.Any(fl => fl.Id == listing.Id))
                {
                    buyer.FavoriteListings!.Add(listing);
                }
            }

        }
    }

    public async Task RemoveFavoriteListingsAsync(Guid buyerId, List<Guid> listingIds)
    {
        var buyer = await _dbSet
            .Include(b => b.FavoriteListings)
            .FirstOrDefaultAsync(b => b.Id == buyerId);

        if (buyer != null)
        {
            buyer.FavoriteListings = buyer.FavoriteListings!
                .Where(fl => !listingIds.Contains(fl.Id))
                .ToList();

        }
    }

    public async Task<Buyer?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Id == userId);
    }
}