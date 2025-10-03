using FluentAssertions;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Controllers;
using HRS.API.Services.Interfaces;
using HRS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace HRS.Test.API.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;
    private readonly IUserContextService _userContextService;
    private readonly IConfiguration _configuration;

    public AuthControllerTests()
    {
        _authService = Substitute.For<IAuthService>();
        _userContextService = Substitute.For<IUserContextService>();
        _configuration = Substitute.For<IConfiguration>();
        _controller = new AuthController(_authService, _userContextService, _configuration);
    }

    [Fact]
    public async Task LoginAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var loginDto = new LoginRequestDto { Email = "test@hrs.com", Password = "password" };
        var responseDto = new LoginResponseDto { Token = "token", RefreshToken = "refresh" };
        _authService.LoginAsync(loginDto).Returns(responseDto);

        // Act
        var result = await _controller.LoginAsync(loginDto, null!);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((LoginResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var refreshDto = new RefreshTokenRequestDto { RefreshToken = "refresh" };
        var responseDto = new LoginResponseDto { Token = "token", RefreshToken = "refresh" };
        _authService.RefreshTokenAsync(refreshDto).Returns(responseDto);

        // Act
        var result = await _controller.RefreshTokenAsync(refreshDto, null!);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((LoginResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task LogoutAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var responseDto = new LogoutResponseDto { Message = "Success" };
        _authService.LogoutAsync().Returns(responseDto);

        // Act
        var result = await _controller.LogoutAsync();

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((LogoutResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsOkWithUserDto()
    {
        // Arrange
        var userDto = new UserDto { Id = 1, FirstName = "Test", LastName = "User", Email = "test@hrs.com", Role = nameof(UserRole.Admin) };
        _userContextService.GetUserDtoAsync().Returns(userDto);

        // Act
        var result = await _controller.GetCurrentUserAsync();

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((UserDto)apiResponse?.Data!).Should().BeEquivalentTo(userDto);
    }
}
