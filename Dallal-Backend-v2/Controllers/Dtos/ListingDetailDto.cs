using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;

namespace Dallal_Backend_v2.Controllers.Dtos;

public class ListingDetailDto
{
    public ListingDetailDto(ListingDetail listingDetail)
    {
        Id = listingDetail.Id;
        Name = new LocalizedStringDto(listingDetail.Definition.Name);
        Definition = new DetailsDefinitionDto(listingDetail.Definition);

        OptionId = listingDetail.OptionId;
        if (listingDetail.OptionId == null)
            Value = listingDetail.Value;
        else
        {
            OptionName = new LocalizedStringDto(
                listingDetail.Option?.Name ?? throw new Exception("Option is null")
            );
        }
    }

    public Guid Id { get; set; }
    public DetailsDefinitionDto Definition { get; set; }
    public LocalizedStringDto Name { get; set; }
    public string? Value { get; set; }
    public Guid? OptionId { get; set; }
    public LocalizedStringDto? OptionName { get; set; }
}
