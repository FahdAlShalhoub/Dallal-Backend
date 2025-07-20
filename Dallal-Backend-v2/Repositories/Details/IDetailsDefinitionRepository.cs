using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Details;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Repositories.Details;

public interface IDetailsDefinitionRepository : IRepository<DetailsDefinition>
{
    Task<List<DetailsDefinition>> GetAllDefinitionsWithOptionsAsync();
    Task<DetailsDefinition?> GetDefinitionWithOptionsAsync(Guid id);
    Task UpdateDefinitionWithOptionsAsync(Guid definitionId, UpdateDetailsDefinitionDto updateDto);
    Task<List<DetailsDefinition>> GetDefinitionsForPropertyTypeAsync(PropertyType propertyType);
    Task<List<DetailsDefinition>> GetRequiredDefinitionsAsync();
}
