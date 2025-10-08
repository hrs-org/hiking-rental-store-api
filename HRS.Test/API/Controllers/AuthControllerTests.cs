using FluentAssertions;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Controllers;
using HRS.API.Services.Interfaces;
using HRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace HRS.Test.API.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;
    private readonly IUserContextService _userContextService;

    public AuthControllerTests()
    {
        _authService = Substitute.For<IAuthService>();
        _userContextService = Substitute.For<IUserContextService>();
        _controller = new AuthController(_authService, _userContextService);
        // Setup HttpContext for cookie manipulation
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task LoginAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var loginDto = new LoginRequestDto { Email = "test@hrs.com", Password = "password" };
        var responseDto = new LoginResponseDto { UserId = 1, Token = "token", RefreshToken = "refresh" };
        _authService.LoginAsync(loginDto).Returns(responseDto);

        // Act
        var result = await _controller.LoginAsync(loginDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((LoginResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(new LoginResponseDto { UserId = 1, Token = "token" });
        // Cookie should be set
        var cookie = _controller.Response.Cookies;
        // No direct getter, but can check that Append was called by not throwing
        cookie.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var responseDto = new LoginResponseDto { UserId = 1, Token = "token", RefreshToken = "refresh" };
        _authService.RefreshTokenAsync("refresh").Returns(responseDto);
        // Set cookie in request using Cookie header
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refresh_token=refresh";

        // Act
        var result = await _controller.RefreshTokenAsync();

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
        // Set cookie in request for delete using Cookie header
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refresh_token=refresh";

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

    [Fact]
    public async Task RefreshTokenAsync_ReturnsUnauthorized_WhenCookieMissing()
    {
        // Arrange: No refresh_token cookie set
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "";

        // Act
        var result = await _controller.RefreshTokenAsync();

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        var apiResponse = unauthorizedResult.Value as dynamic;
        ((string)apiResponse?.Message!).Should().Be("Missing refresh token");
    }

    [Fact]
    public async Task VerifyEmailAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var requestDto = new EmailVerificationRequestDto { VerificationToken = "token" };
        var responseDto = new EmailVerificationResponseDto { IsVerified = true, Message = "Verified" };
        _authService.VerifyEmailAsync(requestDto).Returns(responseDto);

        // Act
        var result = await _controller.VerifyEmailAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((EmailVerificationResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task ResendVerificationAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var requestDto = new ResendVerificationRequestDto { Email = "test@hrs.com" };
        _authService.ResendVerificationEmailAsync(requestDto).Returns(true);

        // Act
        var result = await _controller.ResendVerificationAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((bool)apiResponse?.Data!).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var requestDto = new ChangePasswordRequestDto { CurrentPassword = "old", NewPassword = "new" };
        var responseDto = new ChangePasswordResponseDto { UserId = 1, PasswordChangedAtUtc = DateTime.UtcNow };
        _authService.ChangePasswordAsync(requestDto).Returns(responseDto);

        // Act
        var result = await _controller.ChangePasswordAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((ChangePasswordResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto, options => options.Excluding(x => x.PasswordChangedAtUtc));
    }

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsOkWithResponse()
    {
        // Arrange
        var requestDto = new ForgotPasswordRequestDto { Email = "test@hrs.com" };
        var responseDto = new ForgotPasswordResponseDto { IsSuccess = true, Message = "If the email is registered, a password reset link will be sent." };
        _authService.ForgotPasswordAsync(requestDto).Returns(responseDto);

        // Act
        var result = await _controller.ForgotPasswordAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((ForgotPasswordResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsOkWithSuccessResponse()
    {
        // Arrange
        var requestDto = new ResetPasswordRequestDto
        {
            Email = "test@hrs.com",
            Token = "reset-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };
        var responseDto = new ResetPasswordResponseDto { IsSuccess = true, Message = "Password has been reset successfully." };
        _authService.ResetPasswordAsync(requestDto).Returns(responseDto);

        // Act
        var result = await _controller.ResetPasswordAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((ResetPasswordResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsOkWithFailureResponse_WhenPasswordMismatch()
    {
        // Arrange
        var requestDto = new ResetPasswordRequestDto
        {
            Email = "test@hrs.com",
            Token = "reset-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "DifferentPassword123!"
        };
        var responseDto = new ResetPasswordResponseDto { IsSuccess = false, Message = "Passwords do not match." };
        _authService.ResetPasswordAsync(requestDto).Returns(responseDto);

        // Act
        var result = await _controller.ResetPasswordAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((ResetPasswordResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsOkWithFailureResponse_WhenTokenInvalid()
    {
        // Arrange
        var requestDto = new ResetPasswordRequestDto
        {
            Email = "test@hrs.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };
        var responseDto = new ResetPasswordResponseDto { IsSuccess = false, Message = "Invalid or expired password reset token." };
        _authService.ResetPasswordAsync(requestDto).Returns(responseDto);

        // Act
        var result = await _controller.ResetPasswordAsync(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult.Value as dynamic;
        ((ResetPasswordResponseDto)apiResponse?.Data!).Should().BeEquivalentTo(responseDto);
    }
}
