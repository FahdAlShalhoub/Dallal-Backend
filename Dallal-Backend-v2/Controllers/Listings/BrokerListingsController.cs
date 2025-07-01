using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Controllers.Listings.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("listings")]
[Authorize(Roles = "Broker")]
public class BrokerListingsController(
    DatabaseContext _context,
    SubmissionService _submissionService,
    S3 _s3Service
) : DallalController
{
    [HttpPost("documents/upload-documents")]
    public async Task<PresignedUrlDto> UploadDocuments([FromBody] UploadDocumentRequest request)
    {
        var presignedUrl = await _s3Service.GetPresignedUrl(request.FileName, "brokers/" + UserId);
        return presignedUrl;
    }

    [HttpPost]
    public async Task CreateListing([FromBody] CreateEditListingDto listingDto)
    {
        await ValidateDetails(listingDto.Details, listingDto.PropertyType);
        var listing = new Listing() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };

        SetData(listingDto, listing);
        await UpsertSubmission(listing);
    }

    [HttpPut("{id}")]
    public async Task UpdateListing(Guid id, [FromBody] CreateEditListingDto listingDto)
    {
        var listing = await _context.Listings.Include(i => i.Details).FirstAsync(i => i.Id == id);

        await ValidateDetails(listingDto.Details, listingDto.PropertyType);

        var newListing = new Listing();
        newListing.Id = id;
        newListing.CreatedAt = listing.CreatedAt;
        SetData(listingDto, newListing);
        newListing.UpdatedAt = DateTime.UtcNow;
        if (newListing.BrokerId != UserId)
            throw new UnauthorizedAccessException(
                $"You do not have permission to update this listing."
            );
        await UpsertSubmission(newListing, listing);
    }

    private void SetData(CreateEditListingDto listingDto, Listing listing)
    {
        listing.Name = listingDto.Name;
        listing.Description = listingDto.Description;
        listing.BrokerId = UserId;
        listing.AreaId = listingDto.AreaId;
        listing.Currency = listingDto.Currency;
        listing.PricePerContract = listingDto.PricePerContract;
        listing.BedroomCount = listingDto.BedroomCount;
        listing.BathroomCount = listingDto.BathroomCount;
        listing.AreaInMetersSq = listingDto.AreaInMetersSq;
        listing.ListingType = listingDto.ListingType;
        listing.PropertyType = listingDto.PropertyType;
        listing.RentalContractPeriod = listingDto.RentalContractPeriod;
        listing.Location = new Point(listingDto.Location.Latitude, listingDto.Location.Longitude);
        listing.Details =
            listingDto
                .Details?.Select(detail => new ListingDetail
                {
                    Id = detail.Id ?? Guid.NewGuid(),
                    DefinitionId = detail.DefinitionId,
                    OptionId = detail.OptionId,
                    Value = detail.Value,
                })
                .ToList() ?? [];
        listing.Status = ListingStatus.Active;
        listing.Images = listingDto
            .Images.Select(image => new Document(
                image.FileName,
                image.NameInBucket,
                image.PlaceHolderNameInBucket
            ))
            .ToList();
        listing.Videos = listingDto
            .Videos.Select(video => new Document(
                video.FileName,
                video.NameInBucket,
                video.PlaceHolderNameInBucket
            ))
            .ToList();
    }

    private async Task UpsertSubmission(Listing newListing, Listing? oldListing = null)
    {
        await _submissionService.UpsertSubmission(
            SubmissionType.Listing,
            newListing.Id,
            oldListing,
            newListing
        );

        await _context.SaveChangesAsync();
    }

    private async Task ValidateDetails(List<DetailsDto> details, PropertyType propertyType)
    {
        var definitions = await _context
            .DetailsDefinitions.Where(d => !d.IsHidden)
            .Include(d => d.Options)
            .Where(d =>
                d.PropertyTypes!.Count == 0
                || d.PropertyTypes == null
                || d.PropertyTypes.Contains(propertyType)
            )
            .ToListAsync();

        foreach (var inputDetail in details)
        {
            var definition = definitions.FirstOrDefault(d => d.Id == inputDetail.DefinitionId);
            if (definition == null)
                throw new ValidationException(
                    $"Definition with id {inputDetail.DefinitionId} not found"
                );

            if (definition.Type == DetailDefinitionType.MultiSelect)
            {
                if (inputDetail.OptionId == null)
                    throw new ValidationException(
                        $"Option with id {inputDetail.OptionId} not found"
                    );

                var option = definition.Options?.FirstOrDefault(o => o.Id == inputDetail.OptionId);
                if (option == null)
                    throw new ValidationException(
                        $"Option with id {inputDetail.OptionId} not found"
                    );
                continue;
            }

            if (inputDetail.OptionId != null)
                throw new ValidationException(
                    $"Option with id {inputDetail.OptionId} is not allowed"
                );
            if (definition.Type == DetailDefinitionType.Boolean)
            {
                Console.WriteLine($"inputDetail.Value '{inputDetail.Value}'");
                if (!bool.TryParse(inputDetail.Value, out var boolean))
                    throw new ValidationException(
                        $"Value {inputDetail.Value} is not a valid boolean"
                    );
                continue;
            }

            if (definition.Type == DetailDefinitionType.Number)
            {
                if (!decimal.TryParse(inputDetail.Value, out var number))
                    throw new ValidationException(
                        $"Value {inputDetail.Value} is not a valid number"
                    );
                continue;
            }

            if (definition.Type == DetailDefinitionType.Text)
            {
                // Only validate non-empty text for required fields
                if (definition.IsRequired && string.IsNullOrEmpty(inputDetail.Value))
                    throw new ValidationException(
                        $"Value for required field cannot be null or empty"
                    );
                continue;
            }
            if (definition.Type == DetailDefinitionType.Year)
            {
                if (!int.TryParse(inputDetail.Value, out var year))
                    throw new ValidationException($"Value {inputDetail.Value} is not a valid year");
                continue;
            }
        }

        var requiredDefinitions = await _context
            .DetailsDefinitions.Where(d => d.IsRequired)
            .ToListAsync();

        foreach (var requiredDefinition in requiredDefinitions)
        {
            var detail = details.FirstOrDefault(d => d.DefinitionId == requiredDefinition.Id);
            if (detail == null)
                throw new ValidationException(
                    $"Detail with definition id {requiredDefinition.Id} is required"
                );
        }
    }

    [HttpGet("my-listings")]
    public async Task<PaginatedList<ListingDto>> MyListings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] QueryListingStatus status = QueryListingStatus.Active
    )
    {
        List<QueryDtos.ListingQueryDto> listings;
        int totalCount;
        if (
            status == QueryListingStatus.Active
            || status == QueryListingStatus.Archived
            || status == QueryListingStatus.Completed
        )
        {
            (listings, totalCount) = await GetMyListingsFromListings(
                page,
                pageSize,
                status switch
                {
                    QueryListingStatus.Active => ListingStatus.Active,
                    QueryListingStatus.Completed => ListingStatus.Completed,
                    QueryListingStatus.Archived => ListingStatus.Archived,
                    _ => throw new ArgumentException("Invalid status"),
                }
            );
        }
        else
        {
            (listings, totalCount) = await GetMyListingsFromSubmissions(
                page,
                pageSize,
                status switch
                {
                    QueryListingStatus.Pending => SubmissionStatus.Pending,
                    QueryListingStatus.Rejected => SubmissionStatus.Rejected,
                    _ => throw new ArgumentException("Invalid status"),
                }
            );
        }

        var dtos = await Task.WhenAll(
            listings.Select(l => ListingMapper.SelectToDto(l, _s3Service))
        );
        return new PaginatedList<ListingDto>([.. dtos], page, totalCount, pageSize);
    }

    private async Task<(
        List<QueryDtos.ListingQueryDto> listings,
        int totalCount
    )> GetMyListingsFromListings(int page, int pageSize, ListingStatus status)
    {
        IQueryable<Listing> query = _context
            .Listings.AsQueryable()
            .Where(l => l.BrokerId == UserId)
            .Where(l => l.Status == status);

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(ListingMapper.SelectToQueryDto(null))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await query.CountAsync();
        return (listings, totalCount);
    }

    private async Task<(
        List<QueryDtos.ListingQueryDto> listings,
        int totalCount
    )> GetMyListingsFromSubmissions(int page, int pageSize, SubmissionStatus status)
    {
        var query = _context
            .Submissions.AsQueryable()
            .Where(s => s.Type == SubmissionType.Listing && s.Status == status);

        var submissions = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var existingListings = await _context
            .Listings.Where(l => submissions.Select(s => s.ReferenceId).Contains(l.Id))
            .Include(l => l.Details)
            .ToDictionaryAsync(i => i.Id);

        var listings = submissions
            .Select(s => s.GetNewValue<Listing>())
            .Where(l => l != null)
            .Cast<Listing>()
            .ToList();

        var areasIds = listings.Select(l => l.AreaId).Distinct();
        var brokerIds = listings.Select(l => l.BrokerId).Distinct();

        var areas = await _context
            .Areas.Where(a => areasIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);
        var brokers = await _context
            .Brokers.Where(b => brokerIds.Contains(b.Id))
            .Include(b => b.User)
            .ToDictionaryAsync(b => b.Id);

        foreach (var listing in listings)
        {
            listing.Area = areas[listing.AreaId];
            listing.Broker = brokers[listing.BrokerId];
        }

        var compiled = ListingMapper.SelectToQueryDto(null).Compile();

        var totalCount = await query.CountAsync();
        return (listings.Select(compiled).ToList(), totalCount);
    }

    [HttpGet("my-listing/{id}")]
    public async Task<ListingDetailedDto?> GetListing(Guid id)
    {
        var existingListing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == id);

        var submission = await _context.Submissions.FirstOrDefaultAsync(s =>
            s.Type == SubmissionType.Listing
            && s.ReferenceId == id
            && s.Status == SubmissionStatus.Pending
        );

        var dto = await ListingMapper.MapToDto(
            submission?.GetNewValue<Listing>() ?? existingListing,
            _context,
            _s3Service
        );
        if (dto!.Broker.Id != UserId)
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to view this listing."
            );
        }
        return dto;
    }
}

public enum QueryListingStatus
{
    Pending,
    Active,
    Rejected,
    Completed,
    Archived,
}
