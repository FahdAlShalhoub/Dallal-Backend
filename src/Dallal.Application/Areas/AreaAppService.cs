using System;
using System.Linq;
using System.Threading.Tasks;
using Dallal.Areas.Dtos;
using Dallal.Localization;
using Dallal.Localization.Dtos;
using Dallal.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Dallal.Areas;

[ApiExplorerSettings(GroupName = "Admin")]
[Authorize(DallalPermissions.Areas.Default)]
public class AreaAppService
    : CrudAppService<
        Area,
        AreaDto,
        Guid,
        GetAreaListInput,
        CreateUpdateAreaDto,
        CreateUpdateAreaDto
    >,
        IAreaAppService
{
    public AreaAppService(IRepository<Area, Guid> repository)
        : base(repository)
    {
        CreatePolicyName = DallalPermissions.Areas.Create;
        UpdatePolicyName = DallalPermissions.Areas.Update;
        DeletePolicyName = DallalPermissions.Areas.Delete;
        GetPolicyName = DallalPermissions.Areas.Default;
        GetListPolicyName = DallalPermissions.Areas.Default;
    }

    protected override async Task<IQueryable<Area>> CreateFilteredQueryAsync(GetAreaListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Name),
            x => x.Name.ToString().Contains(input.Name!)
        );

        return query;
    }

    protected override async Task<Area> MapToEntityAsync(CreateUpdateAreaDto createInput)
    {
        var area = new Area(GuidGenerator.Create())
        {
            Name = ObjectMapper.Map<LocalizedStringDto, LocalizedString>(createInput.Name),
        };

        if (createInput.ParentId.HasValue)
        {
            var parent = await Repository.GetAsync(createInput.ParentId.Value);
            area.Parent = parent;
        }

        return area;
    }

    protected override async Task MapToEntityAsync(CreateUpdateAreaDto updateInput, Area entity)
    {
        entity.Name = ObjectMapper.Map<LocalizedStringDto, LocalizedString>(updateInput.Name);

        if (updateInput.ParentId.HasValue)
        {
            var parent = await Repository.GetAsync(updateInput.ParentId.Value);
            entity.Parent = parent;
        }
        else
        {
            entity.Parent = null;
        }
    }

    protected override async Task DeleteByIdAsync(Guid id)
    {
        // Check if area has children
        var queryable = await Repository.GetQueryableAsync();
        var hasChildren = queryable.Any(x => x.Parent!.Id == id);

        if (hasChildren)
        {
            throw new BusinessException("Areas:CannotDeleteAreaWithChildren");
        }

        await base.DeleteByIdAsync(id);
    }

    public virtual async Task<ListResultDto<AreaDto>> GetChildrenAsync(Guid? parentId)
    {
        var queryable = await Repository.GetQueryableAsync();

        if (parentId.HasValue)
        {
            queryable = queryable.Where(x => x.Parent!.Id == parentId.Value);
        }
        else
        {
            queryable = queryable.Where(x => x.Parent == null);
        }

        var areas = await AsyncExecuter.ToListAsync(queryable);

        return new ListResultDto<AreaDto>(
            ObjectMapper.Map<
                System.Collections.Generic.List<Area>,
                System.Collections.Generic.List<AreaDto>
            >(areas)
        );
    }
}
