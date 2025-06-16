using System.Linq.Expressions;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("listings")]
public class ListingsController(DatabaseContext _context) : DallalController
{
    [HttpGet("recent")]
    public async Task<GetRecentListingsResponse> GetRecentListings()
    {
        var query = _context.Listings.AsQueryable();
        query = query.Where(listing => listing.CreatedAt > DateTime.UtcNow.AddDays(-4));
        var count = await query.CountAsync();
        var listings = await query
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Definition)
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Option)
            .Select(ListingMapper.SelectToDto(UserIdOrNull))
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync();

        return new GetRecentListingsResponse
        {
            RecentListingsCount = count,
            ListingsList = listings,
        };
    }

    [HttpGet(Name = "GetListings")]
    public async Task<PaginatedList<ListingDto>> Listings([FromQuery] ListingsSearchDto searchParams)
    {
        var query = _context.Listings.AsQueryable();
        query = await ConstructFilter(
            query,
            searchParams.BedroomCount,
            searchParams.BathroomCount,
            searchParams.MinPrice,
            searchParams.MaxPrice,
            searchParams.MinArea,
            searchParams.MaxArea,
            searchParams.PropertyTypes,
            searchParams.ListingTypes,
            searchParams.AreaIds,
            searchParams.RentalContractPeriods,
            searchParams.Details
        );

        var listings = await query
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Definition)
            .Include(listing => listing.Details)
            .ThenInclude(detail => detail.Option)
            .OrderBy(b => b.Id)
            .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .Select(ListingMapper.SelectToDto(UserIdOrNull))
            .ToListAsync();

        var count = await query.CountAsync();

        return new PaginatedList<ListingDto>(
            listings,
            searchParams.PageNumber,
            count,
            searchParams.PageSize
        );
    }

    private async Task<IQueryable<Listing>> ConstructFilter(
        IQueryable<Listing> query,
        int? bedroomCount,
        int? bathroomCount,
        int? minPrice,
        int? maxPrice,
        int? minArea,
        int? maxArea,
        List<PropertyType>? propertyTypes,
        List<ListingType>? listingTypes,
        List<Guid>? areaIds,
        List<RentalContractPeriod>? rentalContractPeriods,
        List<DetailSearchDto>? details
    )
    {
        query = await FilterDetails(query, details);
        query = await FilterAreas(query, areaIds);
        query = query.WhereIf(
            bedroomCount != null,
            listing => listing.BedroomCount == bedroomCount
        );
        query = query.WhereIf(
            bathroomCount != null,
            listing => listing.BathroomCount == bathroomCount
        );
        query = query.WhereIf(minPrice != null, listing => listing.PricePerContract >= minPrice);
        query = query.WhereIf(maxPrice != null, listing => listing.PricePerContract <= maxPrice);
        query = query.WhereIf(minArea != null, listing => listing.AreaInMetersSq >= minArea);
        query = query.WhereIf(maxArea != null, listing => listing.AreaInMetersSq <= maxArea);
        query = query.WhereIf(
            !propertyTypes.IsNullOrEmpty(),
            listing => propertyTypes!.Contains(listing.PropertyType)
        );
        query = query.WhereIf(
            !listingTypes.IsNullOrEmpty(),
            listing => listingTypes!.Contains(listing.ListingType)
        );
        query = query.WhereIf(
            !rentalContractPeriods.IsNullOrEmpty(),
            listing =>
                listing.RentalContractPeriod != null
                && rentalContractPeriods!.Contains(listing.RentalContractPeriod.Value)
        );
        return query;
    }

    private async Task<IQueryable<Listing>> FilterAreas(
        IQueryable<Listing> query,
        List<Guid>? areaIds
    )
    {
        if (areaIds.IsNullOrEmpty())
            return query;
        var leafAreas = await GetLeafAreas(areaIds!);
        var leafAreaIds = leafAreas.Select(area => area.Id).ToList();
        return query.Where(listing => leafAreaIds.Contains(listing.AreaId));
    }

    private async Task<List<Area>> GetLeafAreas(List<Guid> areaIds, int depth = 0)
    {
        const int maxDepth = 5; // Prevent infinite recursion
        if (depth > maxDepth)
            throw new Exception("Max depth reached while fetching leaf areas");
        var areas = await _context
            .Areas.Where(area => areaIds!.Contains(area.Id))
            .Include(i => i.Children)
            .ToListAsync();

        var allAreas = areas.ToList(); //clone

        var parentAreas = areas.Where(area => !area.Children.IsNullOrEmpty()).ToList();
        if (!parentAreas.IsNullOrEmpty())
        {
            var leafAreasInChildren = await GetLeafAreas(
                parentAreas.SelectMany(area => area.Children).Select(i => i.Id).ToList(),
                depth + 1
            );
            allAreas.AddRange(areas.Where(leafAreasInChildren.Contains));
        }
        return allAreas.Where(i => i.Children.IsNullOrEmpty()).ToList();
    }

    private async Task<IQueryable<Listing>> FilterDetails(
        IQueryable<Listing> query,
        List<DetailSearchDto>? details
    )
    {
        var definitions =
            details != null
                ? await _context
                    .DetailsDefinitions.Where(detail =>
                        details.Any(d => d.DetailDefinitionId == detail.Id)
                    )
                    .ToListAsync()
                : [];

        foreach (var definition in definitions)
        {
            var input = details!.Single(d => d.DetailDefinitionId == definition.Id);
            if (definition.Type == DetailDefinitionType.MultiSelect)
                query = ApplySelectQuery(query, definition, input);
            else if (definition.Type == DetailDefinitionType.Boolean)
                query = query.Where(listing =>
                    listing.Details.Any(d =>
                        d.DefinitionId == definition.Id
                        && bool.Parse(d.Value!) == bool.Parse(input.Values![0])
                    )
                );
            else if (definition.Type == DetailDefinitionType.Number)
                query = query.Where(listing =>
                    listing.Details.Any(d =>
                        d.DefinitionId == definition.Id && input.Values!.Contains(d.Value!)
                    )
                );
            else if (definition.Type == DetailDefinitionType.Text) { }
            // not supported
            else if (definition.Type == DetailDefinitionType.Year)
                query = query.Where(listing =>
                    listing.Details.Any(d =>
                        d.DefinitionId == definition.Id && input.Values!.Contains(d.Value!)
                    )
                );
        }

        return query;
    }

    private static IQueryable<Listing> ApplySelectQuery(
        IQueryable<Listing> query,
        Entities.Details.DetailsDefinition definition,
        DetailSearchDto input
    )
    {
        if (definition.SearchBehavior == DetailDefinitionSearchBehavior.And)
        {
            foreach (var selectedInput in input.Options!)
            {
                query = query.Where(listing =>
                    listing.Details.Any(d =>
                        d.DefinitionId == definition.Id && d.OptionId == selectedInput
                    )
                );
            }
        }
        if (definition.SearchBehavior == DetailDefinitionSearchBehavior.Or)
        {
            query = query.Where(listing =>
                listing.Details.Any(d =>
                    d.DefinitionId == definition.Id && input.Options!.Contains(d.OptionId!.Value)
                )
            );
        }

        return query;
    }
}

public class ListingsSearchDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? BedroomCount { get; set; }
    public int? BathroomCount { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public int? MinArea { get; set; }
    public int? MaxArea { get; set; }
    public List<PropertyType>? PropertyTypes { get; set; }
    public List<ListingType>? ListingTypes { get; set; }
    public List<Guid>? AreaIds { get; set; }
    public List<RentalContractPeriod>? RentalContractPeriods { get; set; }
    public List<DetailSearchDto>? Details { get; set; }
}

public class DetailSearchDto
{
    public Guid DetailDefinitionId { get; set; }
    public List<string>? Values { get; set; } = [];
    public List<Guid>? Options { get; set; } = [];
}
