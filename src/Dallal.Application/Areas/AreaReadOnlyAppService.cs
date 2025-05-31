using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Dallal.Areas;

[ApiExplorerSettings(GroupName = "Customer,Broker")]
public class AreaReadOnlyAppService
    : ReadOnlyAppService<Area, AreaDto, Guid, GetAreaListInput>,
        IAreaReadOnlyAppService
{
    public AreaReadOnlyAppService(IRepository<Area, Guid> repository)
        : base(repository) { }

    protected override async Task<IQueryable<Area>> CreateFilteredQueryAsync(GetAreaListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(x => x.Name.ToString().Contains(input.Name));

        return query;
    }
}
