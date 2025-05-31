using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dallal.Areas;

public interface IAreaReadOnlyAppService : IReadOnlyAppService<AreaDto, Guid, GetAreaListInput> { }
