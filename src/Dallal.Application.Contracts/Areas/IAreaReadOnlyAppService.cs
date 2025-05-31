using System;
using Dallal.Areas.Dtos;
using Volo.Abp.Application.Services;

namespace Dallal.Areas;

public interface IAreaReadOnlyAppService : IReadOnlyAppService<AreaDto, Guid, GetAreaListInput> { }
