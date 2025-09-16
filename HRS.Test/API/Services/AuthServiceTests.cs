using FluentAssertions;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace HRS.Test.API;

public class AuthServiceTests
{
    private readonly IAuthService _mockService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpcontextaccessor;

    public AuthServiceTests()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "super_secret_long_key_1234567890",
            ["Jwt:Issuer"] = "HRSApp",
            ["Jwt:Audience"] = "HRSUsers"
        };

        IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
        _userRepository = Substitute.For<IUserRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _httpcontextaccessor = Substitute.For<IHttpContextAccessor>();
        _mockService = new AuthService(mockConfig, _userRepository, _tokenService, _httpcontextaccessor);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "Admin123!";
        var user = new User
        {
            Id = 1,
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@hrs.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsVerified = true,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var accessToken = "Generated Access Token";

        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _tokenService.GenerateAccessToken(user).Returns(accessToken);

        var dto = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        // Act
        var result = await _mockService.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@hrs.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            Role = UserRole.Admin
        };

        _userRepository.GetByEmailAsync(user.Email).Returns(user);

        var dto = new LoginRequestDto
        {
            Email = user.Email,
            Password = "WrongPassword"
        };

        // Act
        Func<Task> act = async () => await _mockService.LoginAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*incorrect*");
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorized()
    {
        // Arrange
        _userRepository.GetByEmailAsync("nobody@hrs.com").Returns((User?)null);

        var dto = new LoginRequestDto
        {
            Email = "nobody@hrs.com",
            Password = "AnyPassword"
        };

        // Act
        Func<Task> act = async () => await _mockService.LoginAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*incorrect*");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenValid()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@hrs.com",
            RefreshToken = "oldRefresh",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        var requestDto = new RefreshTokenRequestDto
        {
            UserId = user.Id,
            RefreshToken = user.RefreshToken
        };

        _userRepository.GetByIdAsync(user.Id).Returns(user);

        _tokenService.GenerateAccessToken(user).Returns("newAccessToken");
        _tokenService.GenerateRefreshToken().Returns("newRefreshToken");

        // Act
        var result = await _mockService.RefreshTokenAsync(requestDto);

        // Assert
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("newAccessToken", result.Token);
        Assert.Equal("newRefreshToken", result.RefreshToken);

        await _userRepository.Received(1).UpdateUserAsync(Arg.Is<User>(u =>
            u.RefreshToken == "newRefreshToken" &&
            u.RefreshTokenExpiry > DateTime.UtcNow));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var requestDto = new RefreshTokenRequestDto
        {
            UserId = 99,
            RefreshToken = "any"
        };
        _userRepository.GetByIdAsync(99).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _mockService.RefreshTokenAsync(requestDto));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenRefreshTokenMismatch()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@hrs.com",
            RefreshToken = "oldRefresh",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        var requestDto = new RefreshTokenRequestDto
        {
            UserId = 1,
            RefreshToken = "wrongToken"
        };

        _userRepository.GetByIdAsync(1).Returns(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _mockService.RefreshTokenAsync(requestDto));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenRefreshTokenExpired()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@hrs.com",
            RefreshToken = "oldRefresh",
            RefreshTokenExpiry = DateTime.UtcNow.AddHours(-1)
        };

        var requestDto = new RefreshTokenRequestDto
        {
            UserId = 1,
            RefreshToken = "oldRefresh"
        };

        _userRepository.GetByIdAsync(1).Returns(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _mockService.RefreshTokenAsync(requestDto));
    }


    [Fact]
    public async Task LogoutAsync_WhenUserFound_ClearsRefreshTokenAndUpdatesUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@hrs.com",
            RefreshToken = "oldRefresh",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal };
        _httpcontextaccessor.HttpContext.Returns(context);

        _userRepository.GetByIdAsync(Arg.Any<int>()).Returns(user);

        // Act
        await _mockService.LogoutAsync();

        // Assert
        await _userRepository.Received(1).UpdateUserAsync(Arg.Is<User>(u =>
            u.Id == user.Id &&
            string.IsNullOrEmpty(u.RefreshToken) &&
            u.RefreshTokenExpiry == null));
    }

    [Fact]
    public async Task LogoutAsync_WhenUserNotFound_DoesNotThrowOrUpdate()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "999") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal };
        _httpcontextaccessor.HttpContext.Returns(context);

        _userRepository.GetByIdAsync(Arg.Any<int>()).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _mockService.LogoutAsync());
        await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LogoutAsync_WhenHttpContextIsNull_ThrowsUnauthorized()
    {
        // Arrange
        _httpcontextaccessor.HttpContext.Returns((HttpContext?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _mockService.LogoutAsync());
        
        await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LogoutAsync_WhenUserClaimsAreEmpty_ThrowsUnauthorized()
    {
        // Arrange
        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity());
        var context = new DefaultHttpContext { User = principal };
        _httpcontextaccessor.HttpContext.Returns(context);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _mockService.LogoutAsync());
        
        await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LogoutAsync_WhenUserIdIsInvalidFormat_ThrowsUnauthorized()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.NameIdentifier, 
            "invalid-id") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        _httpcontextaccessor.HttpContext.Returns(context);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _mockService.LogoutAsync());
        
        await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }
}
