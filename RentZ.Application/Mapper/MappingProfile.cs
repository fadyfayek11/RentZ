using AutoMapper;
using RentZ.Domain.Entities;
using RentZ.DTO.Property;

namespace RentZ.Application.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AddingProperty, Property>()
            .ForMember(dest => dest.PropertyUtilities,src => src.Ignore());
       
        CreateMap<Property, GetPropertyDetails>()
            .ForMember(dest => dest.PropertyUtilities, src => src.Ignore())
            .ForMember(dest => dest.City, src => src.Ignore());

        CreateMap<Property, GetProperties>()
            .ForMember(dest => dest.City, src => src.Ignore())
            .ForMember(dest => dest.CoverImageUrl, src => src.Ignore());
       
       
    }
}