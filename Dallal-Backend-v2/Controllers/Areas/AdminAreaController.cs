using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Controllers;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers.Areas;

[Route("admin/areas")]
[Authorize(Roles = "admin")]
public class AdminAreaController(DatabaseContext _context) : DallalController
{
    [HttpPost]
    public async Task<AreaDto> CreateArea([FromBody] CreateAreaRequest request)
    {
        var area = new Area
        {
            Id = Guid.NewGuid(),
            Name = new LocalizedString { Values = request.Name.Values },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Set parent if provided
        if (request.ParentId.HasValue)
        {
            var parent = await _context.Areas.FindAsync(request.ParentId.Value);
            if (parent == null)
            {
                throw new ValidationException("Parent area not found");
            }
            area.Parent = parent;
        }

        _context.Areas.Add(area);
        await _context.SaveChangesAsync();

        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            Parent =
                area.Parent != null
                    ? new AreaDto
                    {
                        Id = area.Parent.Id,
                        Name = new LocalizedStringDto(area.Parent.Name),
                        CreatedAt = area.Parent.CreatedAt,
                    }
                    : null,
            CreatedAt = area.CreatedAt,
        };
    }

    [HttpGet("{id}")]
    public async Task<AreaDto> GetArea(Guid id)
    {
        var area = await _context.Areas.Include(a => a.Parent).FirstOrDefaultAsync(a => a.Id == id);

        if (area == null)
        {
            throw new EntityNotFoundException($"Area with ID {id} not found");
        }

        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            Parent =
                area.Parent != null
                    ? new AreaDto
                    {
                        Id = area.Parent.Id,
                        Name = new LocalizedStringDto(area.Parent.Name),
                        CreatedAt = area.Parent.CreatedAt,
                    }
                    : null,
            CreatedAt = area.CreatedAt,
        };
    }

    // Update an existing area
    [HttpPut("{id}")]
    public async Task<AreaDto> UpdateArea(Guid id, [FromBody] UpdateAreaRequest request)
    {
        var area = await _context.Areas.Include(a => a.Parent).FirstOrDefaultAsync(a => a.Id == id);

        if (area == null)
        {
            throw new EntityNotFoundException($"Area with ID {id} not found");
        }

        // Update name
        area.Name = new LocalizedString { Values = request.Name.Values };
        area.UpdatedAt = DateTime.UtcNow;

        // Update parent if provided
        if (request.ParentId.HasValue)
        {
            // Prevent circular references
            if (request.ParentId.Value == id)
            {
                throw new ValidationException("An area cannot be its own parent");
            }

            // Check if the new parent would create a circular reference
            if (await WouldCreateCircularReference(id, request.ParentId.Value))
            {
                throw new ValidationException(
                    "Setting this parent would create a circular reference"
                );
            }

            var parent = await _context.Areas.FindAsync(request.ParentId.Value);
            if (parent == null)
            {
                throw new ValidationException("Parent area not found");
            }
            area.Parent = parent;
        }
        else
        {
            area.Parent = null;
        }

        await _context.SaveChangesAsync();

        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            Parent =
                area.Parent != null
                    ? new AreaDto
                    {
                        Id = area.Parent.Id,
                        Name = new LocalizedStringDto(area.Parent.Name),
                        CreatedAt = area.Parent.CreatedAt,
                    }
                    : null,
            CreatedAt = area.CreatedAt,
        };
    }

    // Delete an area
    [HttpDelete("{id}")]
    public async Task DeleteArea(Guid id)
    {
        var area = await _context
            .Areas.Include(a => a.Children)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (area == null)
        {
            throw new EntityNotFoundException($"Area with ID {id} not found");
        }

        // Check if area has children
        if (area.Children.Any())
        {
            throw new InvalidOperationException(
                "Cannot delete area that has child areas. Delete or reassign child areas first."
            );
        }

        // Check if area is referenced by any listings
        var hasListings = await _context.Listings.AnyAsync(l => l.AreaId == id);
        if (hasListings)
        {
            throw new InvalidOperationException(
                "Cannot delete area that is referenced by listings."
            );
        }

        _context.Areas.Remove(area);
        await _context.SaveChangesAsync();
    }

    // Helper method to check for circular references
    private async Task<bool> WouldCreateCircularReference(Guid areaId, Guid proposedParentId)
    {
        var current = await _context
            .Areas.Include(a => a.Parent)
            .FirstOrDefaultAsync(a => a.Id == proposedParentId);

        while (current?.Parent != null)
        {
            if (current.Parent.Id == areaId)
            {
                return true;
            }
            current = await _context
                .Areas.Include(a => a.Parent)
                .FirstOrDefaultAsync(a => a.Id == current.Parent.Id);
        }

        return false;
    }
}

// DTOs for create and update operations
public class CreateAreaRequest
{
    public LocalizedStringDto Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
}

public class UpdateAreaRequest
{
    public LocalizedStringDto Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
}
