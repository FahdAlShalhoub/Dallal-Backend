using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("details")]
public class DetailsController : DallalController
{
    private readonly DatabaseContext _context;

    public DetailsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<List<DetailsDefinitionDto>> GetDetails()
    {
        var detailsDefinitions = await _context
            .DetailsDefinitions.Include(i => i.Options)
            .ToListAsync();

        return [.. detailsDefinitions.Select(dd => new DetailsDefinitionDto(dd))];
    }
}
