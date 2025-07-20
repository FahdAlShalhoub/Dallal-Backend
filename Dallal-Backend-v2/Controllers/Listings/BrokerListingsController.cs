using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Controllers.Listings.Dtos;
using Dallal_Backend_v2.Controllers.Listings.QueryDtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Helpers.EntityDtoMappers;
using Dallal_Backend_v2.Repositories.Areas;
using Dallal_Backend_v2.Repositories.Brokers;
using Dallal_Backend_v2.Repositories.Details;
using Dallal_Backend_v2.Repositories.Listings;
using Dallal_Backend_v2.Repositories.Submissions;
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
    IListingRepository _listingRepository,
    ISubmissionRepository _submissionRepository,
    IDetailsDefinitionRepository _detailsDefinitionRepository,
    IAreaRepository _areaRepository,
    IBrokerRepository _brokerRepository,
    DatabaseContext _context, // TODO: Remove when ListingMapper.MapToDto is refactored to use repositories
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
        var listing = await _listingRepository.GetListingWithDetailsAsync(id);
        if (listing == null)
            throw new EntityNotFoundException(typeof(Listing), id);

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
        listing.Location = new Point(listingDto.Location.Latitude, listingDto.Location.Longitude)
        {
            SRID = 4326,
        };
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

    }

    private async Task ValidateDetails(List<DetailsDto> details, PropertyType propertyType)
    {
        var definitions = await _detailsDefinitionRepository.GetDefinitionsForPropertyTypeAsync(
            propertyType
        );

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

        var requiredDefinitions = await _detailsDefinitionRepository.GetRequiredDefinitionsAsync();

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
        List<ListingQueryDto> listings;
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
                    QueryListingStatus.Cancelled => SubmissionStatus.Cancelled,
                    _ => throw new ArgumentException("Invalid status"),
                }
            );
        }

        var dtos = await Task.WhenAll(
            listings.Select(l => ListingMapper.SelectToDto(l, _s3Service))
        );
        return new PaginatedList<ListingDto>([.. dtos], page, totalCount, pageSize);
    }

    private async Task<(List<ListingQueryDto> listings, int totalCount)> GetMyListingsFromListings(
        int page,
        int pageSize,
        ListingStatus status
    )
    {
        var result = await _listingRepository.GetBrokerListingsPaginatedAsync(
            UserId,
            status,
            page,
            pageSize
        );

        var listings = result
            .Items.Select(l => ListingMapper.SelectToQueryDto(null).Compile()(l))
            .ToList();

        return (listings, (int)result.Count);
    }

    private async Task<(
        List<ListingQueryDto> listings,
        int totalCount
    )> GetMyListingsFromSubmissions(int page, int pageSize, SubmissionStatus status)
    {
        var result = await _submissionRepository.GetSubmissionsByTypeAndStatusAsync(
            SubmissionType.Listing,
            status,
            page,
            pageSize
        );

        var existingListings = await _listingRepository.GetListingsByIdsWithDetailsAsync(
            result.Items.Select(s => s.ReferenceId).ToList()
        );

        var listings = result
            .Items.Select(s => s.GetNewValue<Listing>())
            .Where(l => l != null)
            .Cast<Listing>()
            .ToList();

        var areasIds = listings.Select(l => l.AreaId).Distinct().ToList();
        var brokerIds = listings.Select(l => l.BrokerId).Distinct().ToList();

        var areas = await _areaRepository.GetAreasByIdsAsync(areasIds);
        var brokers = await _brokerRepository.GetBrokersByIdsWithUserAsync(brokerIds);

        foreach (var listing in listings)
        {
            listing.Area = areas[listing.AreaId];
            listing.Broker = brokers[listing.BrokerId];
        }

        var compiled = ListingMapper.SelectToQueryDto(null).Compile();

        return (listings.Select(compiled).ToList(), (int)result.Count);
    }

    [HttpGet("my-listing/{id}")]
    public async Task<ListingDetailedDto?> GetListing(Guid id)
    {
        var existingListing = await _listingRepository.FindAsync(id);

        var submission = await _submissionRepository.GetSubmissionByTypeAndReferenceIdAsync(
            SubmissionType.Listing,
            id
        );

        // TODO: Refactor ListingMapper.MapToDto to use repositories instead of DatabaseContext
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

    [HttpPost("my-listing/{id}/cancel")]
    public async Task CancelListingSubmission(Guid id)
    {
        var submission = await _submissionRepository.FirstOrDefaultAsync(s =>
            s.Type == SubmissionType.Listing
            && s.ReferenceId == id
            && s.Status == SubmissionStatus.Pending
        );

        if (submission == null)
            throw new EntityNotFoundException(typeof(Submission), id);

        var listing = submission.GetNewValue<Listing>();
        if (listing?.BrokerId != UserId)
            throw new UnauthorizedAccessException(
                "You do not have permission to cancel this submission."
            );

        await _submissionService.CancelSubmission(submission.Id);
    }

    [HttpPost("my-listing/{id}/republish")]
    public async Task RepublishListingSubmission(Guid id)
    {
        var submission = await _submissionRepository.FirstOrDefaultAsync(s =>
            s.Type == SubmissionType.Listing
            && s.ReferenceId == id
            && s.Status == SubmissionStatus.Cancelled
        );

        if (submission == null)
            throw new EntityNotFoundException(typeof(Submission), id);

        var listing = submission.GetNewValue<Listing>();
        if (listing?.BrokerId != UserId)
            throw new UnauthorizedAccessException(
                "You do not have permission to republish this submission."
            );

        submission.Status = SubmissionStatus.Pending;
    }

    [HttpPost("my-listing/{id}/archive")]
    public async Task ArchiveListing(Guid id)
    {
        var listing = await _listingRepository.FindAsync(id);

        if (listing == null)
            throw new EntityNotFoundException(typeof(Listing), id);

        if (listing.BrokerId != UserId)
            throw new UnauthorizedAccessException(
                "You do not have permission to archive this listing."
            );

        if (listing.Status != ListingStatus.Active)
            throw new InvalidOperationException("Only active listings can be archived.");

        await _listingRepository.ArchiveListingAsync(id);
    }

    [HttpPost("my-listing/{id}/unarchive")]
    public async Task UnarchiveListing(Guid id)
    {
        var listing = await _listingRepository.FindAsync(id);

        if (listing == null)
            throw new EntityNotFoundException(typeof(Listing), id);

        if (listing.BrokerId != UserId)
            throw new UnauthorizedAccessException(
                "You do not have permission to unarchive this listing."
            );

        if (listing.Status != ListingStatus.Archived)
            throw new InvalidOperationException("Only archived listings can be unarchived.");

        await _listingRepository.UnarchiveListingAsync(id);
    }
}

public enum QueryListingStatus
{
    Pending,
    Active,
    Rejected,
    Completed,
    Archived,
    Cancelled,
}
