using FluentAssertions;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Controllers;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace HRS.Test.API.Controllers;

public class ItemControllerTests
{
    private readonly ItemController _controller;
    private readonly IItemService _itemService;

    public ItemControllerTests()
    {
        _itemService = Substitute.For<IItemService>();
        _controller = new ItemController(_itemService);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsOkWithList()
    {
        // Arrange
        var items = new List<ItemResponseDto> { new() { Id = 1 }, new() { Id = 2 } };
        _itemService.GetItemsAsync().Returns(items);

        // Act
        var result = await _controller.GetItemsAsync();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((IEnumerable<ItemResponseDto>)apiResponse?.Data!).Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetItemAsync_ReturnsOkWithItem()
    {
        // Arrange
        var item = new ItemResponseDto { Id = 1 };
        _itemService.GetItemAsync(1).Returns(item);

        // Act
        var result = await _controller.GetItemAsync(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((ItemResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task AddNewItem_ReturnsCreatedAtAction()
    {
        // Arrange
        var addDto = new AddItemRequestDto { Name = "Test" };
        var created = new ItemResponseDto { Id = 1, Name = "Test" };
        _itemService.CreateItemAsync(addDto).Returns(created);

        // Act
        var result = await _controller.AddNewItem(addDto);

        // Assert
        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        var apiResponse = createdResult.Value as dynamic;
        ((ItemResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(created);
        createdResult.RouteValues?["id"].Should().Be(created.Id);
    }

    [Fact]
    public async Task UpdateItemAsync_ReturnsNoContent()
    {
        // Arrange
        var updateDto = new UpdateItemRequestDto { Id = 1, Name = "Updated" };
        _itemService.UpdateItemAsync(updateDto).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateItemAsync(1, updateDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        updateDto.Id.Should().Be(1);
        await _itemService.Received(1).UpdateItemAsync(updateDto);
    }

    [Fact]
    public async Task DeleteItemAsync_ReturnsNoContent()
    {
        // Arrange
        _itemService.DeleteItemAsync(1).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteItemAsync(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _itemService.Received(1).DeleteItemAsync(1);
    }
}
