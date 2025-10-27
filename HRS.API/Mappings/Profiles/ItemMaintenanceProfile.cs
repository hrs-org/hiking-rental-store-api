using AutoMapper;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.Domain.Entities;

namespace HRS.API.Mappings.Profiles;

public class ItemMaintenanceProfile : Profile
{
    public ItemMaintenanceProfile()
    {
        CreateMap<ItemMaintenance, ItemMaintenanceResponseDto>()
            .ForMember(i => i.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
