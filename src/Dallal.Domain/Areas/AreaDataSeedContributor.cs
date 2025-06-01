using System;
using System.Threading.Tasks;
using Dallal.Localization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Dallal.Areas;

/* Creates initial area data that is needed for the application
 * to provide basic geographical coverage for Riyadh.
 */
public class AreaDataSeedContributor(IRepository<Area, Guid> _areaRepository)
    : IDataSeedContributor,
        ITransientDependency
{
    // Public constants for area IDs to be referenced by other seed contributors
    public static readonly Guid RiyadhId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid NorthRiyadhId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid EastRiyadhId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid AlYasmeenId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid AlNaseemId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid AlRawdahId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateAreasAsync();
    }

    private async Task CreateAreasAsync()
    {
        // Create main area: Riyadh
        await CreateAreaAsync(RiyadhId, "Riyadh", "الرياض", null);

        // Create sub-areas under Riyadh
        await CreateAreaAsync(NorthRiyadhId, "North Riyadh", "شمال الرياض", RiyadhId);
        await CreateAreaAsync(EastRiyadhId, "East Riyadh", "شرق الرياض", RiyadhId);

        // Create neighborhoods under North Riyadh
        await CreateAreaAsync(AlYasmeenId, "Al Yasmeen", "الياسمين", NorthRiyadhId);

        // Create neighborhoods under East Riyadh
        await CreateAreaAsync(AlNaseemId, "Al Naseem", "النسيم", EastRiyadhId);
        await CreateAreaAsync(AlRawdahId, "Al Rawdah", "الروضة", EastRiyadhId);
    }

    private async Task<Area> CreateAreaAsync(Guid id, string nameEn, string nameAr, Guid? parentId)
    {
        // Check if area already exists
        var existingArea = await _areaRepository.FindAsync(id);
        if (existingArea != null)
        {
            return existingArea;
        }

        // Create localized name
        var localizedName = new LocalizedString();
        localizedName.SetValue("en", nameEn);
        localizedName.SetValue("ar", nameAr);

        // Get parent area if specified
        Area? parent = null;
        if (parentId.HasValue)
        {
            parent = await _areaRepository.GetAsync(parentId.Value);
        }

        // Create new area
        var area = new Area(id) { Name = localizedName, Parent = parent };

        return await _areaRepository.InsertAsync(area, autoSave: true);
    }
}
