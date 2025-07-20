using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;
using Dallal_Backend_v2.Repositories.Details;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("details")]
public class AdminDetailsController : DallalController
{
    private readonly IDetailsDefinitionRepository _detailsRepository;

    public AdminDetailsController(IDetailsDefinitionRepository detailsRepository)
    {
        _detailsRepository = detailsRepository;
    }

    [HttpPost]
    public async Task<DetailsDefinitionDto> CreateDetailsDefinition(
        CreateDetailsDefinitionDto createDto
    )
    {
        var detailsDefinition = await _detailsRepository.AddAsync(
            new DetailsDefinition
            {
                Id = Guid.NewGuid(),
                Name = new LocalizedString { Values = createDto.Name.Values },
                Type = createDto.Type,
                SearchBehavior = createDto.SearchBehavior,
                PropertyTypes = createDto.PropertyTypes,
                IsHidden = createDto.IsHidden,
                Options =
                [
                    .. createDto.Options.Select(option => new DetailsDefinitionOption
                    {
                        Name = new LocalizedString { Values = option.Name.Values },
                    }),
                ],
            }
        );
        return new DetailsDefinitionDto(detailsDefinition);
    }

    [HttpPut("{id}")]
    public async Task<DetailsDefinitionDto> UpdateDetailsDefinition(
        Guid id,
        UpdateDetailsDefinitionDto updateDetailsDefinitionDto
    )
    {
        await _detailsRepository.UpdateDefinitionWithOptionsAsync(id, updateDetailsDefinitionDto);
        var updatedDefinition = await _detailsRepository.GetDefinitionWithOptionsAsync(id);

        if (updatedDefinition is null)
            throw new KeyNotFoundException($"Details definition with ID {id} was not found.");

        return new DetailsDefinitionDto(updatedDefinition);
    }
}
