using Dallal_Backend_v2.Controllers.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("details")]
public class DetailsController : ControllerBase
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

        return
        [
            .. detailsDefinitions.Select(dd => new DetailsDefinitionDto
            {
                Id = dd.Id,
                Name = new(dd.Name),
                Type = dd.Type,
                Options =
                [
                    .. dd.Options.Select(o => new DetailsDefinitionOptionDto
                    {
                        Id = o.Id,
                        Name = new(o.Name),
                    }),
                ],
            }),
        ];
    }
}
