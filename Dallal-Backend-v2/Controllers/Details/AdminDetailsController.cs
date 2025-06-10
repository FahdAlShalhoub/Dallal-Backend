using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
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
            Options = createDetailsDefinitionDto
                .Options.Select(option => new DetailsDefinitionOption
                {
                    Name = new LocalizedString { Values = option.Name.Values },
                })
                .ToList(),
        };

        await _context.DetailsDefinitions.AddAsync(detailsDefinition);
        await _context.SaveChangesAsync();

        return MapToDto(detailsDefinition);
    }

    [HttpPut("{id}")]
    public async Task<DetailsDefinitionDto> UpdateDetailsDefinition(
        Guid id,
        UpdateDetailsDefinitionDto updateDetailsDefinitionDto
    )
    {
        var detailsDefinition = await GetDetailsDefinitionByIdAsync(id);

        UpdateBasicProperties(detailsDefinition, updateDetailsDefinitionDto);
        await UpdateOptionsAsync(detailsDefinition, updateDetailsDefinitionDto.Options);

        await _context.SaveChangesAsync();

        return MapToDto(detailsDefinition);
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
    }

    private async Task UpdateOptionsAsync(
        DetailsDefinition detailsDefinition,
        IEnumerable<UpdateDetailsDefinitionOptionDto> incomingOptions
    )
    {
        var incomingOptionIds = incomingOptions
            .Where(option => option.Id.HasValue)
            .Select(option => option.Id!.Value)
            .ToHashSet();

        RemoveDeletedOptions(detailsDefinition, incomingOptionIds);
        await UpdateAndAddOptionsAsync(detailsDefinition, incomingOptions);
    }

    private void RemoveDeletedOptions(
        DetailsDefinition detailsDefinition,
        HashSet<Guid> incomingOptionIds
    )
    {
        var optionsToRemove = detailsDefinition
            .Options.Where(option => !incomingOptionIds.Contains(option.Id))
            .ToList();

        foreach (var optionToRemove in optionsToRemove)
        {
            detailsDefinition.Options.Remove(optionToRemove);
            _context.DetailsDefinitionOptions.Remove(optionToRemove);
        }
    }

    private async Task UpdateAndAddOptionsAsync(
        DetailsDefinition detailsDefinition,
        IEnumerable<UpdateDetailsDefinitionOptionDto> incomingOptions
    )
    {
        foreach (var incomingOption in incomingOptions)
        {
            if (incomingOption.Id.HasValue)
            {
                UpdateExistingOption(detailsDefinition, incomingOption);
            }
            else
            {
                AddNewOption(detailsDefinition, incomingOption);
            }
        }
    }

    private static void UpdateExistingOption(
        DetailsDefinition detailsDefinition,
        UpdateDetailsDefinitionOptionDto incomingOption
    )
    {
        var existingOption = detailsDefinition.Options.FirstOrDefault(option =>
            option.Id == incomingOption.Id!.Value
        );

        if (existingOption != null)
        {
            existingOption.Name = new LocalizedString { Values = incomingOption.Name.Values };
        }
    }

    private static void AddNewOption(
        DetailsDefinition detailsDefinition,
        UpdateDetailsDefinitionOptionDto incomingOption
    )
    {
        var newOption = new DetailsDefinitionOption
        {
            Id = Guid.NewGuid(),
            Name = new LocalizedString { Values = incomingOption.Name.Values },
        };

        detailsDefinition.Options.Add(newOption);
    }

    private static DetailsDefinitionDto MapToDto(DetailsDefinition detailsDefinition)
    {
        return new DetailsDefinitionDto
        {
            Id = detailsDefinition.Id,
            Name = new LocalizedStringDto(detailsDefinition.Name),
            Type = detailsDefinition.Type,
            Options = detailsDefinition
                .Options.Select(option => new DetailsDefinitionOptionDto
                {
                    Id = option.Id,
                    Name = new LocalizedStringDto(option.Name),
                })
                .ToList(),
        };
    }
}
