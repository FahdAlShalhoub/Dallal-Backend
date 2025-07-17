using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Controllers;
using Dallal_Backend_v2.Controllers.Areas.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Repositories;
using Dallal_Backend_v2.Repositories.Areas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers.Areas;

[Route("admin/areas")]
[Authorize(Roles = "admin")]
public class AdminAreaController(IAreaRepository _areaRepository) : DallalController
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
            var parent = await _areaRepository.FindAsync(request.ParentId.Value);
            if (parent == null)
            {
                throw new ValidationException("Parent area not found");
            }
            area.Parent = parent;
        }

        await _areaRepository.AddAsync(area);

        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            FullName = new LocalizedStringDto(area.FullName),
            Parent =
                area.Parent != null
                    ? new AreaDto
                    {
                        Id = area.Parent.Id,
                        Name = new LocalizedStringDto(area.Parent.Name),
                        FullName = new LocalizedStringDto(area.Parent.FullName),
                        CreatedAt = area.Parent.CreatedAt,
                    }
                    : null,
            CreatedAt = area.CreatedAt,
        };
    }

    [HttpGet("{id}")]
    public async Task<AreaDto> GetArea(Guid id)
    {
        var area = await _areaRepository.GetAsync(id);

        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            FullName = new LocalizedStringDto(area.FullName),
            Parent =
                area.Parent != null
                    ? new AreaDto
                    {
                        Id = area.Parent.Id,
                        Name = new LocalizedStringDto(area.Parent.Name),
                        FullName = new LocalizedStringDto(area.Parent.FullName),
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
        var area = await _areaRepository.GetAsync(id);

        // Update name
        area.Name = new LocalizedString { Values = request.Name.Values };
        area.UpdatedAt = DateTime.UtcNow;

        // Update parent if provided
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                throw new ValidationException("An area cannot be its own parent");

            if (await WouldCreateCircularReferenceAsync(area, request.ParentId.Value))
            {
                throw new ValidationException(
                    "Setting this parent would create a circular reference"
                );
            }
            area.Parent = await _areaRepository.GetAsync(request.ParentId.Value);
        }
        else
        {
            area.Parent = null;
        }

        await _areaRepository.UpdateAsync(area);

        return new AreaDto
        {
            Id = area.Id,
            Name = new LocalizedStringDto(area.Name),
            FullName = new LocalizedStringDto(area.FullName),
            Parent =
                area.Parent != null
                    ? new AreaDto
                    {
                        Id = area.Parent.Id,
                        Name = new LocalizedStringDto(area.Parent.Name),
                        FullName = new LocalizedStringDto(area.Parent.FullName),
                        CreatedAt = area.Parent.CreatedAt,
                    }
                    : null,
            CreatedAt = area.CreatedAt,
        };
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(Area area, Guid value)
    {
        var parent = await _areaRepository.FindAsync(value);
        while (parent != null)
        {
            if (parent.Id == area.Id)
                return true; 
            
            parent = await _areaRepository.GetAsync(parent.Id);
        }
        return false; 
    }

    // Delete an area
    [HttpDelete("{id}")]
    public async Task DeleteArea(Guid id)
    {
        var area = await _areaRepository.GetAsync(id);
     
        // Check if area has children
        if (area.Children.Any())
        {
            throw new InvalidOperationException(
                "Cannot delete area that has child areas. Delete or reassign child areas first."
            );
        }
        await _areaRepository.DeleteAsync(area);
    }
}
