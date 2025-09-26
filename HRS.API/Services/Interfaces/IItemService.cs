using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Services.Interfaces;

public interface IItemService
{
    Task<ItemResponseDto> GetItemAsync(int id);
    Task<IEnumerable<ItemResponseDto>> GetItemsAsync();
    Task<ItemResponseDto> CreateItemAsync(AddItemRequestDto dto);
    Task UpdateItemAsync(UpdateItemRequestDto dto);
    Task DeleteItemAsync(int id);
}
