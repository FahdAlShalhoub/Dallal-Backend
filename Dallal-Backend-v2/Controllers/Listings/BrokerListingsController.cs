using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Controllers.Listings.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Services;
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
            Location = new Point(listingDto.Location.Latitude, listingDto.Location.Longitude),
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
                if (string.IsNullOrEmpty(inputDetail.Value))
                    throw new ValidationException($"Value {inputDetail.Value} is not a valid text");
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
}
