using System;
using AutoMapper;
using Dallal.Areas;
using Dallal.Localization;

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

        // Mapping between LocalizedString and LocalizedStringDto
        CreateMap<LocalizedString, LocalizedStringDto>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values));

        CreateMap<LocalizedStringDto, LocalizedString>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values));
    }
}
