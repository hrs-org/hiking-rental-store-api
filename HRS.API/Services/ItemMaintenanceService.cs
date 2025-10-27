using AutoMapper;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services.Interfaces;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class ItemMaintenanceService : IItemMaintenanceService
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;

    public ItemMaintenanceService(
        IItemMaintenanceRepository itemMaintenanceRepository,
        IItemRepository itemRepository,
        IUserContextService userContextService,
        IMapper mapper)
    {
        _itemMaintenanceRepository = itemMaintenanceRepository;
        _itemRepository = itemRepository;
        _userContextService = userContextService;
        _mapper = mapper;
    }

    public async Task<ItemMaintenanceResponseDto> GetAsync(int id)
    {
        var record = await _itemMaintenanceRepository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException("Maintenance record not found.");

        return _mapper.Map<ItemMaintenanceResponseDto>(record);
    }

    public async Task<IEnumerable<ItemMaintenanceResponseDto>> GetAllAsync()
    {
        var records = await _itemMaintenanceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records);
    }

    public async Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(ItemMaintenanceRequestDto request)
    {
        var user = await _userContextService.GetUserAsync();

        await using var tx = await _itemMaintenanceRepository.BeginTransactionAsync();

        try
        {
            var record = await _itemMaintenanceRepository.GetByIdAsync(request.Id)
                         ?? throw new KeyNotFoundException("Maintenance record not found.");

            if (record.Type != ItemMaintenanceType.Repair)
                throw new InvalidOperationException("Only 'Repair' maintenance can be marked as fixed.");

            if (request.QuantityFixed <= 0 || request.QuantityFixed > record.Quantity)
                throw new ArgumentException("Invalid quantity to fix.");

            var item = await _itemRepository.GetByIdAsync(record.ItemId)
                       ?? throw new KeyNotFoundException("Item not found.");

            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedById = user.Id;

            _itemMaintenanceRepository.Remove(record);
            
            var response = _mapper.Map<ItemMaintenanceResponseDto>(record);
            response.QuantityFixed = request.QuantityFixed;

            await _itemMaintenanceRepository.SaveChangesAsync();
            await tx.CommitAsync();

            return response;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
