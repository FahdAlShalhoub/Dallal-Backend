using Dallal_Backend_v2.Controllers;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Areas;

[Route("areas")]
public class AreaController(DatabaseContext _context) : DallalController
{
    // list paginated areas
    [HttpGet]
    public async Task<List<AreaDto>> GetAreas(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var areas = await _context
            .Areas.Include(a => a.Parent)
            .Where(i => string.IsNullOrEmpty(search) || ((string)i.Name).Contains(search))
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return areas
            .Select(a => new AreaDto
            {
                Id = a.Id,
                Name = new LocalizedStringDto(a.Name),
                Parent =
                    a.Parent != null
                        ? new AreaDto
                        {
                            Id = a.Parent.Id,
                            Name = new LocalizedStringDto(a.Parent.Name),
                            CreatedAt = a.Parent.CreatedAt,
                        }
                        : null,
                CreatedAt = a.CreatedAt,
            })
            .ToList();
    }
}

public class AreaDto
{
    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public AreaDto? Parent { get; set; }
}
