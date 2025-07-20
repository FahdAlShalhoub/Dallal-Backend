using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;
using Dallal_Backend_v2.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Details;

public class DetailsDefinitionRepository
    : Repository<DetailsDefinition>,
        IDetailsDefinitionRepository
{
    public DetailsDefinitionRepository(DatabaseContext context)
        : base(context) { }

    public async Task<List<DetailsDefinition>> GetAllDefinitionsWithOptionsAsync()
    {
        return await _dbSet.Include(d => d.Options).ToListAsync();
    }

    public async Task<DetailsDefinition?> GetDefinitionWithOptionsAsync(Guid id)
    {
        return await _dbSet.Include(d => d.Options).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task UpdateDefinitionWithOptionsAsync(
        Guid definitionId,
        UpdateDetailsDefinitionDto updateDto
    )
    {
        // Load the entity without navigation properties to avoid tracking conflicts
        var detailsDefinition = await _dbSet.FirstOrDefaultAsync(d => d.Id == definitionId);

        if (detailsDefinition is null)
        {
            throw new KeyNotFoundException(
                $"Details definition with ID {definitionId} was not found."
            );
        }

        // Update basic properties
        detailsDefinition.Name = new LocalizedString { Values = updateDto.Name.Values };
        detailsDefinition.Type = updateDto.Type;
        detailsDefinition.SearchBehavior = updateDto.SearchBehavior;
        detailsDefinition.PropertyTypes = updateDto.PropertyTypes;
        detailsDefinition.IsHidden = updateDto.IsHidden;


        // Handle options separately to avoid tracking conflicts
        await UpdateOptionsDirectlyAsync(definitionId, updateDto.Options);
    }

    public async Task<List<DetailsDefinition>> GetDefinitionsForPropertyTypeAsync(
        PropertyType propertyType
    )
    {
        return await _dbSet
            .Where(d => !d.IsHidden)
            .Include(d => d.Options)
            .Where(d =>
                d.PropertyTypes!.Count == 0
                || d.PropertyTypes == null
                || d.PropertyTypes.Contains(propertyType)
            )
            .ToListAsync();
    }

    public async Task<List<DetailsDefinition>> GetRequiredDefinitionsAsync()
    {
        return await _dbSet.Where(d => d.IsRequired).ToListAsync();
    }

    private async Task UpdateOptionsDirectlyAsync(
        Guid definitionId,
        List<UpdateDetailsDefinitionOptionDto> incomingOptions
    )
    {
        var incomingOptionIds = incomingOptions
            .Where(option => option.Id.HasValue)
            .Select(option => option.Id!.Value)
            .ToHashSet();

        var definition = await _dbSet
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == definitionId);

        if (definition?.Options == null)
            return;

        var optionsToRemove = definition
            .Options.Where(option => !incomingOptionIds.Contains(option.Id))
            .ToList();

        foreach (var optionToRemove in optionsToRemove)
        {
            definition.Options.Remove(optionToRemove);
            _context.DetailsDefinitionOptions.Remove(optionToRemove);
        }

        foreach (var incomingOption in incomingOptions)
        {
            if (incomingOption.Id.HasValue)
            {
                var existingOption = definition.Options.First(o => o.Id == incomingOption.Id.Value);
                existingOption.Name = new LocalizedString { Values = incomingOption.Name.Values };
            }
            else
            {
                definition.Options.Add(
                    new DetailsDefinitionOption
                    {
                        Id = Guid.NewGuid(),
                        Name = new LocalizedString { Values = incomingOption.Name.Values },
                    }
                );
            }
        }

    }
}
