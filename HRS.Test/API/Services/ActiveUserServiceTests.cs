using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace HRS.Test.API.Services;

public class ActiveUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IActiveUserService _activeUserService;

    public ActiveUserServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _userRepository = Substitute.For<IUserRepository>();
        _activeUserService = new ActiveUserService(_httpContextAccessor, _userRepository);
    }

    [Fact]
    public async Task GetActiveUserAsync_ReturnsUser_WhenAuthenticated()
    {
        // Arrange
        var userId = 1;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = Substitute.For<ClaimsIdentity>();
        identity.IsAuthenticated.Returns(true);
        identity.FindFirst(ClaimTypes.NameIdentifier).Returns(claims[0]);
        var principal = new ClaimsPrincipal(identity);
        var context = Substitute.For<HttpContext>();
        context.User.Returns(principal);
        _httpContextAccessor.HttpContext.Returns(context);

        var user = new User { Id = userId, FirstName = "Test", LastName = "User", Email = "test@hrs.com", PasswordHash = "hash", IsVerified = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _activeUserService.GetActiveUserAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
    }

    [Fact]
    public async Task GetActiveUserAsync_Throws_WhenNotAuthenticated()
    {
        // Arrange
        var identity = Substitute.For<ClaimsIdentity>();
        identity.IsAuthenticated.Returns(false);
        var principal = new ClaimsPrincipal(identity);
        var context = Substitute.For<HttpContext>();
        context.User.Returns(principal);
        _httpContextAccessor.HttpContext.Returns(context);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _activeUserService.GetActiveUserAsync());
    }

    [Fact]
    public async Task GetActiveUserAsync_Throws_WhenUserIdClaimMissing()
    {
        // Arrange
        var identity = Substitute.For<ClaimsIdentity>();
        identity.IsAuthenticated.Returns(true);
        identity.FindFirst(ClaimTypes.NameIdentifier).Returns((Claim?)null!);
        var principal = new ClaimsPrincipal(identity);
        var context = Substitute.For<HttpContext>();
        context.User.Returns(principal);
        _httpContextAccessor.HttpContext.Returns(context);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _activeUserService.GetActiveUserAsync());
    }

    [Fact]
    public async Task GetActiveUserAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        var userId = 2;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = Substitute.For<ClaimsIdentity>();
        identity.IsAuthenticated.Returns(true);
        identity.FindFirst(ClaimTypes.NameIdentifier).Returns(claims[0]);
        var principal = new ClaimsPrincipal(identity);
        var context = Substitute.For<HttpContext>();
        context.User.Returns(principal);
        _httpContextAccessor.HttpContext.Returns(context);

        _userRepository.GetByIdAsync(userId).Returns((User?)null!);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _activeUserService.GetActiveUserAsync());
    }
}
