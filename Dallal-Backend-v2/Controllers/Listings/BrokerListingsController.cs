using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("listings")]
[Authorize(Roles = "Broker")]
public class BrokerListingsController(
    DatabaseContext _context,
    SubmissionService _submissionService
) : DallalController
{
    [HttpPost]
    public async Task CreateListing([FromBody] CreateEditListingDto listingDto)
    {
        Console.WriteLine("User Claims: ");
        Console.WriteLine(string.Join(',', User.Claims.Select(c => $"{c.Type}: {c.Value}")));
        var listing = new Listing
        {
            Name = listingDto.Name,
            Description = listingDto.Description,
            BrokerId = UserId,
            AreaId = listingDto.AreaId,
            Currency = listingDto.Currency,
            PricePerContract = listingDto.PricePerContract,
            BedroomCount = listingDto.BedroomCount,
            BathroomCount = listingDto.BathroomCount,
            AreaInMetersSq = listingDto.AreaInMetersSq,
            ListingType = listingDto.ListingType,
            PropertyType = listingDto.PropertyType,
            RentalContractPeriod = listingDto.RentalContractPeriod,
            Details =
                listingDto
                    .Details?.Select(detail => new ListingDetail
                    {
                        DefinitionId = detail.DefinitionId,
                        OptionId = detail.OptionId,
                    })
                    .ToList() ?? [],
            Status = ListingStatus.Pending,
        };

        _context.Listings.Add(listing);
        await _submissionService.CreateSubmission(
            SubmissionType.Listing,
            listing.Id,
            null,
            listing
        );
    }
}

public class CreateEditListingDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid AreaId { get; set; }
    public string Currency { get; set; }
    public decimal PricePerContract { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public List<DetailsDto> Details { get; set; }
}

public class DetailsDto
{
    public Guid DefinitionId { get; set; }
    public Guid OptionId { get; set; }
}
