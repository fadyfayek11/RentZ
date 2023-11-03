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
    }
}