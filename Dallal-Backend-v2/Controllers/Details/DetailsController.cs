using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Repositories.Details;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("details")]
public class DetailsController(IDetailsDefinitionRepository _detailsRepository) : DallalController
{
    [HttpGet]
    public async Task<List<DetailsDefinitionDto>> GetDetails()
    {
        var detailsDefinitions = await _detailsRepository.GetAllDefinitionsWithOptionsAsync();

        return [.. detailsDefinitions.Select(dd => new DetailsDefinitionDto(dd))];
    }
}
