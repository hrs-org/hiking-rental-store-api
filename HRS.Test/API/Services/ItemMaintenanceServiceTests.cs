using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;

namespace HRS.Test.API.Services;

public class ItemMaintenanceServiceTests
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;
    private readonly ItemMaintenanceService _service;
    private readonly IUserContextService _userContextService;

    public ItemMaintenanceServiceTests()
    {
        _itemMaintenanceRepository = Substitute.For<IItemMaintenanceRepository>();
        _itemRepository = Substitute.For<IItemRepository>();
        _userContextService = Substitute.For<IUserContextService>();
        _mapper = Substitute.For<IMapper>();
        _service = new ItemMaintenanceService(_itemMaintenanceRepository, _itemRepository, _userContextService, _mapper);
    }

    [Fact]
    public async Task GetAsync_WhenRecordExists_ReturnsMappedDto()
    {
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Repair, Quantity = 2 };
        var dto = new ItemMaintenanceResponseDto { Id = 1 };
        _itemMaintenanceRepository.GetByIdAsync(1).Returns(record);
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(dto);

        var result = await _service.GetAsync(1);
        result.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
    {
        _itemMaintenanceRepository.GetByIdAsync(1).Returns((ItemMaintenance)null!);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var records = new List<ItemMaintenance> { new() { Id = 1 }, new() { Id = 2 } };
        var dtos = new List<ItemMaintenanceResponseDto> { new() { Id = 1 }, new() { Id = 2 } };
        _itemMaintenanceRepository.GetAllAsync().Returns(records);
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records).Returns(dtos);

        var result = await _service.GetAllAsync();
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenValid_UpdatesRecordAndReturnsDto()
    {
        var user = new User { Id = 10 };
        var item = new Item { Id = 5, Quantity = 95 };
        var record = new ItemMaintenance { Id = 1, ItemId = 5, Type = ItemMaintenanceType.Repair, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 3, Remarks = "Fixed" };
        var dto = new ItemMaintenanceResponseDto { Id = 1, QuantityFixed = 3 };
        
        _userContextService.GetUserAsync().Returns(user);
        _itemMaintenanceRepository.GetByIdAsync(1).Returns(record);
        _itemRepository.GetByIdAsync(5).Returns(item);
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(dto);
        var mockTransaction = Substitute.For<IDbContextTransaction>();
        _itemMaintenanceRepository.BeginTransactionAsync().Returns(mockTransaction);

        var result = await _service.MarkAsFixedAsync(request);

        await _itemRepository.Received(1).GetByIdAsync(5);
        
        item.UpdatedById.Should().Be(user.Id);
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        _itemMaintenanceRepository.Received(1).Remove(record);
        
        await _itemMaintenanceRepository.Received(1).SaveChangesAsync();
        await mockTransaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        
        result.Should().BeEquivalentTo(dto);
        result.QuantityFixed.Should().Be(3);
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
    {
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 1 };
        _itemMaintenanceRepository.GetByIdAsync(1).Returns((ItemMaintenance)null!);
        var mockTransaction = Substitute.For<IDbContextTransaction>();
        _itemMaintenanceRepository.BeginTransactionAsync().Returns(mockTransaction);
        
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.MarkAsFixedAsync(request));
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenTypeNotRepair_ThrowsInvalidOperationException()
    {
        var user = new User { Id = 10 };
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Broken, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 1 };
        _userContextService.GetUserAsync().Returns(user);
        _itemMaintenanceRepository.GetByIdAsync(1).Returns(record);
        var mockTransaction = Substitute.For<IDbContextTransaction>();
        _itemMaintenanceRepository.BeginTransactionAsync().Returns(mockTransaction);
        
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MarkAsFixedAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public async Task MarkAsFixedAsync_WhenQuantityInvalid_ThrowsArgumentException(int quantityFixed)
    {
        var user = new User { Id = 10 };
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Repair, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = quantityFixed };
        _userContextService.GetUserAsync().Returns(user);
        _itemMaintenanceRepository.GetByIdAsync(1).Returns(record);
        var mockTransaction = Substitute.For<IDbContextTransaction>();
        _itemMaintenanceRepository.BeginTransactionAsync().Returns(mockTransaction);
        
        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsFixedAsync(request));
    }
}
