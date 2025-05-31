using System;
using System.Threading.Tasks;
using Dallal.Localization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Dallal.DetailDefinition;

/* Creates initial detail definition data that includes
 * all the filter categories and options for property search.
 */
public class DetailsDefinitionDataSeedContributor(
    IRepository<DetailsDefinition, Guid> _detailsDefinitionRepository,
    IRepository<DetailsDefinitionOption, Guid> _detailsDefinitionOptionRepository
) : IDataSeedContributor, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateDetailsDefinitionsAsync();
    }

    private async Task CreateDetailsDefinitionsAsync()
    {
        // Predefined GUIDs for consistency
        var floorId = Guid.Parse("77ed1ba2-e791-52da-a1bc-3a33483ed056");
        var furnishingsId = Guid.Parse("c8845de7-ddf5-5621-bdf8-cc0cc79dd7b7");
        var optionalId = Guid.Parse("316a2ef0-b6bd-5709-9bb5-48839e041ecb");
        var wishesForResidentsId = Guid.Parse("f004971a-d2e1-5af1-ab39-a088a74926c6");
        var locationId = Guid.Parse("fa39a2d8-7d39-5ee7-a597-c0fd4493292e");
        var houseMaterialId = Guid.Parse("64fdd774-3047-53e0-bc58-78d02cc18a5f");

        // Floor
        await CreateDetailsDefinitionAsync(
            floorId,
            "Floor",
            "الطابق",
            MultipleSearchBehavior.Or,
            new[]
            {
                (
                    Guid.Parse("f5ed842a-e1ac-5efd-8c60-6e966842abc2"),
                    "Ground floor",
                    "الطابق الأرضي"
                ),
                (Guid.Parse("f2951035-1129-593d-8e6c-2052d3405fc9"), "First floor", "الطابق الأول"),
                (
                    Guid.Parse("84ed0934-e71a-56f2-ba0b-7d396505f50d"),
                    "2+ floor",
                    "الطابق الثاني أو أعلى"
                ),
                (Guid.Parse("9f97adf6-9be6-523f-bc86-b1e02fff74a5"), "Last floor", "الطابق الأخير"),
            }
        );

        // Furnishings
        await CreateDetailsDefinitionAsync(
            furnishingsId,
            "Furnishings",
            "الأثاث",
            MultipleSearchBehavior.Or,
            new[]
            {
                (Guid.Parse("ea37811e-5d2b-5256-be0f-11c104e29625"), "Furnished", "مفروش"),
                (Guid.Parse("cab1a842-7341-5194-b8a9-515c50ea730d"), "Unfurnished", "غير مفروش"),
                (
                    Guid.Parse("54924250-6702-58d9-addc-6b2e981ca0e2"),
                    "Party furnished",
                    "مفروش جزئياً"
                ),
            }
        );

        // Optional
        await CreateDetailsDefinitionAsync(
            optionalId,
            "Optional",
            "اختياري",
            MultipleSearchBehavior.And,
            new[]
            {
                (
                    Guid.Parse("248c3ae8-9243-5043-8275-40db36547981"),
                    "Air conditioning",
                    "تكييف الهواء"
                ),
                (Guid.Parse("71536350-5ba5-553c-979c-28a7ab833535"), "Dishwasher", "غسالة الصحون"),
            }
        );

        // Wishes for residents
        await CreateDetailsDefinitionAsync(
            wishesForResidentsId,
            "Wishes for residents",
            "رغبات السكان",
            MultipleSearchBehavior.And,
            new[]
            {
                (
                    Guid.Parse("19d52e84-59e1-5df0-8eac-1e2582213958"),
                    "No Children Allowed",
                    "لا يسمح بالأطفال"
                ),
                (
                    Guid.Parse("0d8d40f5-40f5-50f3-9b91-c4c6e898b744"),
                    "No Pets Allowed",
                    "لا يسمح بالحيوانات"
                ),
            }
        );

        // Location
        await CreateDetailsDefinitionAsync(
            locationId,
            "Location",
            "الموقع",
            MultipleSearchBehavior.And,
            new[]
            {
                (
                    Guid.Parse("90af6f66-4139-5144-8826-735325f871bd"),
                    "Near a school",
                    "بالقرب من مدرسة"
                ),
                (
                    Guid.Parse("b6e4ae80-6b0f-553b-b085-679083974e38"),
                    "Near a forest",
                    "بالقرب من غابة"
                ),
                (
                    Guid.Parse("8662a163-80aa-5a0e-920e-1c35c9ed47e8"),
                    "Near a zoo",
                    "بالقرب من حديقة حيوان"
                ),
                (
                    Guid.Parse("18401c6a-7dd7-5fee-84f4-b135cb0c152d"),
                    "Near a university",
                    "بالقرب من جامعة"
                ),
                (
                    Guid.Parse("9e97aead-b090-5679-8cac-6b753720f649"),
                    "Near a café",
                    "بالقرب من مقهى"
                ),
                (
                    Guid.Parse("886df223-7b4e-5240-aca2-c9c90a34fb30"),
                    "Close to a leisure center",
                    "بالقرب من مركز ترفيهي"
                ),
                (
                    Guid.Parse("cf49f099-8c0c-5dfc-bab5-3a3140aa1c07"),
                    "Near a sports field",
                    "بالقرب من ملعب رياضي"
                ),
                (
                    Guid.Parse("1af15c46-3ff5-50bb-bac7-5acdaf121edb"),
                    "Near a swimming pool",
                    "بالقرب من مسبح"
                ),
                (
                    Guid.Parse("4201a8ea-ba90-5b61-8116-581e8ad0a4b7"),
                    "Near a stadium",
                    "بالقرب من استاد"
                ),
                (
                    Guid.Parse("9bd065c0-2f9d-5dba-824b-03fd0bf355c7"),
                    "Near a pharmacy",
                    "بالقرب من صيدلية"
                ),
                (
                    Guid.Parse("ac276991-94b1-5046-8ab0-0d7d074e6653"),
                    "With a view of the mountains",
                    "بإطلالة على الجبال"
                ),
                (
                    Guid.Parse("6be24c77-f626-58ef-9009-64459084c1e8"),
                    "Near a lake",
                    "بالقرب من بحيرة"
                ),
            }
        );

        // House material
        await CreateDetailsDefinitionAsync(
            houseMaterialId,
            "House material",
            "مادة البناء",
            MultipleSearchBehavior.Or,
            new[]
            {
                (Guid.Parse("3ce7b1cb-0c90-5955-88e7-7c449a053421"), "Panel", "لوحات"),
                (Guid.Parse("081d9095-8e4b-54e2-8ab0-4ba68225ecbd"), "Brick", "طوب"),
                (Guid.Parse("92babb13-71b6-5753-8859-ff432a6617c3"), "Monolithic", "خرساني"),
            }
        );
    }

    private async Task CreateDetailsDefinitionAsync(
        Guid id,
        string nameEn,
        string nameAr,
        MultipleSearchBehavior type,
        (Guid Id, string NameEn, string NameAr)[] options
    )
    {
        // Check if details definition already exists
        var existingDefinition = await _detailsDefinitionRepository.FindAsync(id);
        if (existingDefinition != null)
        {
            return;
        }

        // Create localized name for definition
        var localizedName = new LocalizedString();
        localizedName.SetValue("en", nameEn);
        localizedName.SetValue("ar", nameAr);

        // Create new details definition
        var definition = new DetailsDefinition(id)
        {
            Name = localizedName,
            Type = type,
            Options = [],
        };

        // Insert definition first
        await _detailsDefinitionRepository.InsertAsync(definition, autoSave: true);

        // Create options
        foreach (var (optionId, optionNameEn, optionNameAr) in options)
        {
            var existingOption = await _detailsDefinitionOptionRepository.FindAsync(optionId);
            if (existingOption != null)
            {
                continue;
            }

            var optionLocalizedName = new LocalizedString();
            optionLocalizedName.SetValue("en", optionNameEn);
            optionLocalizedName.SetValue("ar", optionNameAr);

            var option = new DetailsDefinitionOption(optionId) { Name = optionLocalizedName };

            await _detailsDefinitionOptionRepository.InsertAsync(option, autoSave: true);

            // Add option to definition
            definition.Options.Add(option);
        }

        await _detailsDefinitionRepository.UpdateAsync(definition, autoSave: true);
    }
}
