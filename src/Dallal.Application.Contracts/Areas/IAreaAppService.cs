using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dallal.Areas;

public interface IAreaAppService
    : ICrudAppService<AreaDto, Guid, GetAreaListInput, CreateUpdateAreaDto, CreateUpdateAreaDto>
{
    Task<ListResultDto<AreaDto>> GetChildrenAsync(Guid? parentId);
}
