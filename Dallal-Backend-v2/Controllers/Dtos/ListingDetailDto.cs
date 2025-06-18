using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;

namespace Dallal_Backend_v2.Controllers.Dtos;

public class ListingDetailDto
{
    public ListingDetailDto(ListingDetail listingDetail)
    {
        Id = listingDetail.Id;
        Name = new LocalizedStringDto(listingDetail.Definition.Name);
        DisplayCategory = listingDetail.Definition.DisplayCategory;
        if (listingDetail.OptionId == null)
            Value = new LocalizedStringDto(
                listingDetail.Value ?? throw new Exception("Value is null")
            );
        else
            Value = new LocalizedStringDto(
                listingDetail.Option?.Name ?? throw new Exception("Option is null")
            );
    }

    public Guid Id { get; set; }
    public LocalizedStringDto Name { get; set; }
    public LocalizedStringDto Value { get; set; }
    public DetailDisplayCategory DisplayCategory { get; set; }
}
