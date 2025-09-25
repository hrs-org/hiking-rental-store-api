using AutoMapper;
using HRS.API.Contracts.DTOs.Item;
using HRS.Domain.Entities;

namespace HRS.API.Mappings.Profiles;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<AddItemRequestDto, Item>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ParentId, opt => opt.Ignore())
            .ForMember(d => d.Parent, opt => opt.Ignore())
            .ForMember(d => d.Children, opt => opt.MapFrom(s => s.Children));

        CreateMap<Item, ItemResponseDto>()
            .ForMember(d => d.Children, opt => opt.MapFrom(s => s.Children));
    }
}
