using AutoMapper;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class ItemService : IItemService
{
    private const string ItemNotFound = "Item not found";
    private readonly IActiveUserService _activeUserService;
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;

    public ItemService(IMapper mapper, IActiveUserService activeUserService, IItemRepository itemRepository)
    {
        _mapper = mapper;
        _activeUserService = activeUserService;
        _itemRepository = itemRepository;
    }

    public async Task<ItemResponseDto> GetItemAsync(int id)
    {
        var res = await _itemRepository.GetByIdWithChildrenAsync(id);
        return res == null ? throw new KeyNotFoundException(ItemNotFound) : _mapper.Map<ItemResponseDto>(res);
    }

    public async Task<IEnumerable<ItemResponseDto>> GetItemsAsync()
    {
        var res = await _itemRepository.GetRootItemsAsync();
        return _mapper.Map<IEnumerable<ItemResponseDto>>(res);
    }

    public async Task<ItemResponseDto> CreateItemAsync(AddItemRequestDto dto)
    {
        var entity = _mapper.Map<Item>(dto);

        var user = _activeUserService.GetActiveUserAsync();

        entity.CreatedById = user.Id;
        entity.CreatedAt = DateTime.UtcNow;
        if (entity.Children.Count > 0)
        {
            entity.Quantity = entity.Children.Sum(c => c.Quantity);
            foreach (var child in entity.Children)
            {
                child.CreatedAt = DateTime.UtcNow;
                child.CreatedById = user.Id;
            }
        }

        await _itemRepository.AddAsync(entity);
        await _itemRepository.SaveChangesAsync();

        var response = _mapper.Map<ItemResponseDto>(entity);
        return response;
    }

    public async Task UpdateItemAsync(UpdateItemRequestDto dto)
    {
        if (!dto.Id.HasValue) throw new KeyNotFoundException(ItemNotFound);

        var existingItem = await _itemRepository.GetByIdWithChildrenAsync(dto.Id.Value) ?? throw new KeyNotFoundException(ItemNotFound);
        var user = await _activeUserService.GetActiveUserAsync();

        existingItem.Name = dto.Name;
        existingItem.Description = dto.Description;
        existingItem.Quantity = dto.Quantity;
        existingItem.Price = dto.Price;
        existingItem.UpdatedBy = user;
        existingItem.UpdatedAt = DateTime.UtcNow;

        var children = dto.Children?
            .ToDictionary(c => c.Id ?? 0) ?? [];

        foreach (var child in existingItem.Children.ToList())
            if (children.TryGetValue(child.Id, out var dtoChild))
            {
                child.Name = dtoChild.Name;
                child.Description = dtoChild.Description;
                child.Quantity = dtoChild.Quantity;
                child.Price = dtoChild.Price;
                child.UpdatedBy = user;
                child.UpdatedAt = DateTime.UtcNow;

                children.Remove(child.Id);
            }
            else
            {
                _itemRepository.Remove(child);
            }

        var newChilds = children.Values.Select(dtoChild => new Item
        {
            Name = dtoChild.Name,
            Description = dtoChild.Description,
            Quantity = dtoChild.Quantity,
            Price = dtoChild.Price,
            ParentId = existingItem.Id,
            CreatedBy = user,
            CreatedAt = DateTime.UtcNow
        });

        foreach (var newChild in newChilds) existingItem.Children.Add(newChild);

        existingItem.Quantity = existingItem.Children.Sum(c => c.Quantity);

        await _itemRepository.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException(ItemNotFound);
        _itemRepository.Remove(item);
        await _itemRepository.SaveChangesAsync();
    }
}
