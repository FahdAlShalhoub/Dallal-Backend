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
        await ValidateDetails(listingDto.Details, listingDto.PropertyType);
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
        await _submissionService.UpsertSubmission(
            SubmissionType.Listing,
            listing.Id,
            null,
            listing
        );
    }

    private async Task ValidateDetails(List<DetailsDto> details, PropertyType propertyType)
    {
        var definitions = await _context
            .DetailsDefinitions.Where(d => !d.IsHidden)
            .Where(d => d.PropertyTypes == null || d.PropertyTypes.Contains(propertyType))
            .ToListAsync();

        foreach (var inputDetail in details)
        {
            var definition = definitions.FirstOrDefault(d => d.Id == inputDetail.DefinitionId);
            if (definition == null)
                throw new Exception($"Definition with id {inputDetail.DefinitionId} not found");

            if (definition.Type == DetailDefinitionType.MultiSelect)
            {
                if (inputDetail.OptionId == null)
                    throw new Exception($"Option with id {inputDetail.OptionId} not found");

                var option = definition.Options?.FirstOrDefault(o => o.Id == inputDetail.OptionId);
                if (option == null)
                    throw new Exception($"Option with id {inputDetail.OptionId} not found");
                continue;
            }

            if (inputDetail.OptionId != null)
                throw new Exception($"Option with id {inputDetail.OptionId} is not allowed");
            if (definition.Type == DetailDefinitionType.Boolean)
            {
                if (!bool.TryParse(inputDetail.Value, out var boolean))
                    throw new Exception($"Value {inputDetail.Value} is not a valid boolean");
                continue;
            }

            if (definition.Type == DetailDefinitionType.Number)
            {
                if (!decimal.TryParse(inputDetail.Value, out var number))
                    throw new Exception($"Value {inputDetail.Value} is not a valid number");
                continue;
            }

            if (definition.Type == DetailDefinitionType.Text)
            {
                if (string.IsNullOrEmpty(inputDetail.Value))
                    throw new Exception($"Value {inputDetail.Value} is not a valid text");
                continue;
            }
            if (definition.Type == DetailDefinitionType.Year)
            {
                if (!int.TryParse(inputDetail.Value, out var year))
                    throw new Exception($"Value {inputDetail.Value} is not a valid year");
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
                throw new Exception(
                    $"Detail with definition id {requiredDefinition.Id} is required"
                );
        }
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
    public Guid? OptionId { get; set; }
    public string? Value { get; internal set; }
}
