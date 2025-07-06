using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("details")]
public class AdminDetailsController : DallalController
{
    private readonly DatabaseContext _context;

    public AdminDetailsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<DetailsDefinitionDto> CreateDetailsDefinition(
        CreateDetailsDefinitionDto createDetailsDefinitionDto
    )
    {
        var detailsDefinition = new DetailsDefinition
        {
            Id = Guid.NewGuid(),
            Name = new LocalizedString { Values = createDetailsDefinitionDto.Name.Values },
            Type = createDetailsDefinitionDto.Type,
            SearchBehavior = createDetailsDefinitionDto.SearchBehavior,
            PropertyTypes = createDetailsDefinitionDto.PropertyTypes,
            IsHidden = createDetailsDefinitionDto.IsHidden,
            Options = createDetailsDefinitionDto
                .Options.Select(option => new DetailsDefinitionOption
                {
                    Name = new LocalizedString { Values = option.Name.Values },
                })
                .ToList(),
        };

        await _context.DetailsDefinitions.AddAsync(detailsDefinition);
        await _context.SaveChangesAsync();

        return new DetailsDefinitionDto(detailsDefinition);
    }

    [HttpPut("{id}")]
    public async Task<DetailsDefinitionDto> UpdateDetailsDefinition(
        Guid id,
        UpdateDetailsDefinitionDto updateDetailsDefinitionDto
    )
    {
        // Load the entity without navigation properties to avoid tracking conflicts
        var detailsDefinition = await _context.DetailsDefinitions.FirstOrDefaultAsync(d =>
            d.Id == id
        );

        if (detailsDefinition is null)
        {
            throw new KeyNotFoundException($"Details definition with ID {id} was not found.");
        }

        // Update basic properties first
        UpdateBasicProperties(detailsDefinition, updateDetailsDefinitionDto);
        await _context.SaveChangesAsync();

        // Handle options separately to avoid tracking conflicts
        await UpdateOptionsDirectlyAsync(id, updateDetailsDefinitionDto.Options);
        await _context.SaveChangesAsync();

        // Reload the entity with options for the response
        var updatedDefinition = await GetDetailsDefinitionByIdAsync(id);
        return new DetailsDefinitionDto(updatedDefinition);
    }

    private async Task<DetailsDefinition> GetDetailsDefinitionByIdAsync(Guid id)
    {
        var detailsDefinition = await _context
            .DetailsDefinitions.Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (detailsDefinition is null)
        {
            throw new KeyNotFoundException($"Details definition with ID {id} was not found.");
        }

        return detailsDefinition;
    }

    private static void UpdateBasicProperties(
        DetailsDefinition detailsDefinition,
        UpdateDetailsDefinitionDto updateDto
    )
    {
        detailsDefinition.Name = new LocalizedString { Values = updateDto.Name.Values };
        detailsDefinition.Type = updateDto.Type;
        detailsDefinition.SearchBehavior = updateDto.SearchBehavior;
        detailsDefinition.PropertyTypes = updateDto.PropertyTypes;
        detailsDefinition.IsHidden = updateDto.IsHidden;
    }

    private async Task UpdateOptionsDirectlyAsync(
        Guid detailsDefinitionId,
        List<UpdateDetailsDefinitionOptionDto> incomingOptions
    )
    {
        var incomingOptionIds = incomingOptions
            .Where(option => option.Id.HasValue)
            .Select(option => option.Id!.Value)
            .ToHashSet();

        // Get the details definition with its options
        var definition = await _context
            .DetailsDefinitions.Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == detailsDefinitionId);

        if (definition?.Options == null)
            return;

        // Remove options that are not referenced by any listings
        var optionsToRemove = definition
            .Options.Where(option => !incomingOptionIds.Contains(option.Id))
            .ToList();

        foreach (var optionToRemove in optionsToRemove)
        {
            // Check if this option is referenced by any listings
            var isReferenced = await _context.ListingDetails.AnyAsync(ld =>
                ld.OptionId == optionToRemove.Id
            );

            if (!isReferenced)
            {
                definition.Options.Remove(optionToRemove);
                _context.DetailsDefinitionOptions.Remove(optionToRemove);
            }
        }

        // Update existing options and add new ones
        foreach (var incomingOption in incomingOptions)
        {
            if (incomingOption.Id.HasValue)
            {
                // Update existing option
                var existingOption = definition.Options.FirstOrDefault(o =>
                    o.Id == incomingOption.Id.Value
                );
                if (existingOption != null)
                {
                    existingOption.Name = new LocalizedString
                    {
                        Values = incomingOption.Name.Values,
                    };
                }
            }
            else
            {
                // Add new option
                var newOption = new DetailsDefinitionOption
                {
                    Id = Guid.NewGuid(),
                    Name = new LocalizedString { Values = incomingOption.Name.Values },
                };
                definition.Options.Add(newOption);
            }
        }
    }
}
