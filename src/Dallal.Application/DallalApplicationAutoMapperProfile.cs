using System;
using AutoMapper;
using Dallal.Areas;
using Dallal.Areas.Dtos;
using Dallal.Listings;
using Dallal.Listings.Dtos;
using Dallal.Localization;
using Dallal.Localization.Dtos;

namespace Dallal;

public class DallalApplicationAutoMapperProfile : Profile
{
    public DallalApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<Area, AreaDto>()
            .ForMember(
                dest => dest.ParentId,
                opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Id : (Guid?)null)
            );
        CreateMap<CreateUpdateAreaDto, Area>();

        // Listing mappings
        CreateMap<Listing, ListingDto>();
        CreateMap<CreateUpdateListingDto, Listing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Area, opt => opt.Ignore())
            .ForMember(dest => dest.Broker, opt => opt.Ignore())
            .ForMember(dest => dest.Details, opt => opt.Ignore());

        CreateMap<ListingDetail, ListingDetailDto>();
        CreateMap<CreateUpdateListingDetailDto, ListingDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Definition, opt => opt.Ignore())
            .ForMember(dest => dest.Option, opt => opt.Ignore());

        // Mapping between LocalizedString and LocalizedStringDto
        CreateMap<LocalizedString, LocalizedStringDto>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values));

        CreateMap<LocalizedStringDto, LocalizedString>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values));
    }
}
