using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API.Services;

public class ItemServiceTests
{
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        _itemRepository = Substitute.For<IItemRepository>();
        _mapper = Substitute.For<IMapper>();
        _service = new ItemService(_mapper, _itemRepository);
    }

    [Fact]
    public async Task GetItemAsync_WhenItemExists_ReturnsMappedDto()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Name = "Tent",
            Description = "This is tent",
            Quantity = 14,
            Price = 10,
            Children =
            [
                new Item
                {
                    Id = 2,
                    Name = "Size XL",
                    Description = "This is tent size XL",
                    Quantity = 14,
                    Price = 0,
                    ParentId = 1
                }
            ]
        };
        var dto = new ItemResponseDto
        {
            Id = 1,
            Name = "Tent",
            Description = "This is tent",
            Quantity = 14,
            Price = 10,
            Children =
            [
                new ItemResponseDto
                {
                    Id = 2,
                    Name = "Size XL",
                    Description = "This is tent size XL",
                    Quantity = 14,
                    Price = 0
                }
            ]
        };
        _itemRepository.GetByIdWithChildrenAsync(1).Returns(item);
        _mapper.Map<ItemResponseDto>(item).Returns(dto);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Should().BeEquivalentTo(dto);
        result.Id.Should().Be(1);
        result.Name.Should().Be("Tent");
        result.Description.Should().Be("This is tent");
        result.Quantity.Should().Be(14);
        result.Price.Should().Be(10);
        result.Children.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetItemAsync_WhenItemNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _ = _itemRepository.GetByIdWithChildrenAsync(1)!.Returns((Item)null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetItemAsync(1));
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsMappedDtos()
    {
        // Arrange
        var items = new List<Item> { new() { Id = 1 }, new() { Id = 2 } };
        var dtos = new List<ItemResponseDto> { new() { Id = 1 }, new() { Id = 2 } };
        _itemRepository.GetRootItemsAsync().Returns(items);
        _mapper.Map<IEnumerable<ItemResponseDto>>(items).Returns(dtos);

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task CreateItemAsync_WithChildren_SetsQuantityAndSaves()
    {
        // Arrange
        var addDto = new AddItemRequestDto
        {
            Name = "Shoes",
            Description = "This is shoes",
            Quantity = 5,
            Price = 10,
            Children = new List<AddItemRequestDto>
            {
                new() { Name = "Size 8", Quantity = 2 },
                new() { Name = "Size 10", Quantity = 3 }
            }
        };
        var entity = new Item
        {
            Name = "Shoes",
            Description = "This is shoes",
            Quantity = 5,
            Price = 10,
            Children = new List<Item>
            {
                new() { Name = "Size 8", Quantity = 2 },
                new() { Name = "Size 10", Quantity = 3 }
            }
        };
        var responseDto = new ItemResponseDto
        {
            Name = "Shoes",
            Description = "This is shoes",
            Quantity = 5,
            Price = 10,
            Children = new List<ItemResponseDto>
            {
                new() { Name = "Size 8", Quantity = 2 },
                new() { Name = "Size 10", Quantity = 3 }
            }
        };
        _mapper.Map<Item>(addDto).Returns(entity);
        _mapper.Map<ItemResponseDto>(entity).Returns(responseDto);

        // Act
        var result = await _service.CreateItemAsync(addDto);

        // Assert
        entity.Quantity.Should().Be(5);
        await _itemRepository.Received(1).AddAsync(entity);
        await _itemRepository.Received(1).SaveChangesAsync();
        result.Should().BeEquivalentTo(responseDto);
        result.Name.Should().Be(addDto.Name);
        result.Description.Should().Be(addDto.Description);
        result.Quantity.Should().Be(addDto.Quantity);
        result.Price.Should().Be(addDto.Price);
        result.Children.Should().HaveCount(addDto.Children.Count);
    }

    [Fact]
    public async Task UpdateItemAsync_UpdatesFieldsAndChildren()
    {
        // Arrange
        var existing = new Item
        {
            Id = 1,
            Name = "Old",
            Description = "OldDesc",
            Quantity = 1,
            Price = 10,
            Children = new List<Item>
            {
                new() { Id = 2, Name = "Child", Quantity = 1, Price = 5, Description = "desc" },
                new() { Id = 3, Name = "Child2", Quantity = 1, Price = 10, Description = "desc" }
            }
        };
        var dto = new UpdateItemRequestDto
        {
            Id = 1,
            Name = "New",
            Description = "NewDesc",
            Quantity = 2,
            Price = 20,
            Children = new List<UpdateItemChildDto>
            {
                new() { Id = 2, Name = "ChildUpdated", Description = "desc2", Quantity = 2, Price = 6 },
                new() { Name = "NewChild", Description = "desc3", Quantity = 3, Price = 7 }
            }
        };
        _itemRepository.GetByIdWithChildrenAsync(1).Returns(existing);

        // Act
        await _service.UpdateItemAsync(dto);

        // Assert
        existing.Name.Should().Be("New");
        existing.Description.Should().Be("NewDesc");
        existing.Price.Should().Be(20);
        existing.Children.Should().ContainSingle(c => c.Name == "ChildUpdated");
        existing.Children.Should().ContainSingle(c => c.Name == "NewChild");
        await _itemRepository.Received(1).SaveChangesAsync();
        _itemRepository.Received(1).Remove(Arg.Any<Item>());
    }

    [Fact]
    public async Task UpdateItemAsync_WhenItemNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _itemRepository.GetByIdWithChildrenAsync(1).Returns((Item)null!);
        var dto = new UpdateItemRequestDto { Id = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateItemAsync(dto));
    }

    [Fact]
    public async Task DeleteItemAsync_RemovesAndSaves()
    {
        // Arrange
        var item = new Item { Id = 1 };
        _itemRepository.GetByIdAsync(1).Returns(item);

        // Act
        await _service.DeleteItemAsync(1);

        // Assert
        _itemRepository.Received(1).Remove(item);
        await _itemRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteItemAsync_WhenItemNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _itemRepository.GetByIdAsync(1).Returns((Item)null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteItemAsync(1));
    }
}
