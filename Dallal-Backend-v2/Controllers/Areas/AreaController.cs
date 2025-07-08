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
    public async Task<PaginatedList<AreaDto>> GetAreas(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var areas = await _context
            .Areas.Include(a => a.Parent)
            .Where(i =>
                string.IsNullOrEmpty(search)
                || ((string)i.Name).ToLower().Contains(search.ToLower())
            )
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var count = await _context
            .Areas.Include(a => a.Parent)
            .Where(i =>
                string.IsNullOrEmpty(search)
                || ((string)i.Name).ToLower().Contains(search.ToLower())
            )
            .CountAsync();

        return new PaginatedList<AreaDto>(
            areas
                .Select(i => new AreaDto()
                {
                    Id = i.Id,
                    Name = new LocalizedStringDto(i.Name),
                    FullName = new LocalizedStringDto(i.FullName),
                    Parent =
                        i.Parent != null
                            ? new AreaDto
                            {
                                Id = i.Parent.Id,
                                Name = new LocalizedStringDto(i.Parent.Name),
                                FullName = new LocalizedStringDto(i.Parent.FullName),
                                CreatedAt = i.Parent.CreatedAt,
                            }
                            : null,
                    CreatedAt = i.CreatedAt,
                })
                .ToList(),
            page,
            pageSize,
            count
        );
    }

    // leaf areas only
    [HttpGet("leafs")]
    public async Task<List<AreaDto>> GetLeafAreas(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var areas = await _context
            .Areas.Include(a => a.Parent)
            .Where(i =>
                string.IsNullOrEmpty(search)
                || ((string)i.FullName).ToLower().Contains(search.ToLower())
            )
            .Where(a => a.Children.Count == 0) // only leaf areas
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return areas
            .Select(a => new AreaDto
            {
                Id = a.Id,
                Name = new LocalizedStringDto(a.Name),
                FullName = new LocalizedStringDto(a.FullName),
                Parent =
                    a.Parent != null
                        ? new AreaDto
                        {
                            Id = a.Parent.Id,
                            Name = new LocalizedStringDto(a.Parent.Name),
                            FullName = new LocalizedStringDto(a.Parent.FullName),
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
    public LocalizedStringDto FullName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public AreaDto? Parent { get; set; }
}
