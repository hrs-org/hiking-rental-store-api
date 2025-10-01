using FluentAssertions;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API.Services;

public class AuthServiceTests
{
    private readonly IAuthService _mockService;
    private readonly IUserContextService _mockUserContextService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSenderService _emailSenderService;

    public AuthServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mockUserContextService = Substitute.For<IUserContextService>();
        _tokenService = Substitute.For<ITokenService>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _emailSenderService = Substitute.For<IEmailSenderService>();
        _mockService = new AuthService(_userRepository, _mockUserContextService, _tokenService, _emailBuilderService, _emailSenderService);
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
            RefreshToken = user.RefreshToken
        };

        _mockUserContextService.GetUserAsync().Returns(user);

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
            RefreshToken = "wrongToken"
        };

        _mockUserContextService.GetUserAsync().Returns(user);

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
            RefreshToken = "oldRefresh"
        };

        _mockUserContextService.GetUserAsync().Returns(user);

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
        _mockUserContextService.GetUserAsync().Returns(user);

        // Act
        await _mockService.LogoutAsync();

        // Assert
        await _userRepository.Received(1).UpdateUserAsync(Arg.Is<User>(u =>
            u.Id == user.Id &&
            string.IsNullOrEmpty(u.RefreshToken) &&
            u.RefreshTokenExpiry == null));
    }

    #region VerifyEmailAsync Tests

    [Fact]
    public async Task VerifyEmailAsync_WhenUserNotFound_ReturnsFailedRedirect()
    {
        // Arrange
        var requestDto = new EmailVerificationRequestDto
        {
            Email = "nonexistent@example.com",
            VerificationToken = "valid-token"
        };
        var frontendUrl = new Uri("http://localhost:4200");

        _userRepository.GetByEmailAsync(requestDto.Email).Returns((User?)null);

        // Act
        var result = await _mockService.VerifyEmailAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsVerified.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenUserAlreadyVerified_ReturnsSuccessRedirect()
    {
        // Arrange
        var requestDto = new EmailVerificationRequestDto
        {
            Email = "verified@example.com",
            VerificationToken = "any-token"
        };
        var frontendUrl = new Uri("http://localhost:4200");

        var user = new User
        {
            Id = 1,
            Email = requestDto.Email,
            IsVerified = true,
            EmailVerificationToken = "any-token"
        };

        _userRepository.GetByEmailAsync(requestDto.Email).Returns(user);

        // Act
        var result = await _mockService.VerifyEmailAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsVerified.Should().BeTrue();
        result.Message.Should().Be("Email is already verified");
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenTokenInvalid_ReturnsFailedRedirect()
    {
        // Arrange
        var requestDto = new EmailVerificationRequestDto
        {
            Email = "user@example.com",
            VerificationToken = "invalid-token"
        };
        var frontendUrl = new Uri("http://localhost:4200");

        var user = new User
        {
            Id = 1,
            Email = requestDto.Email,
            IsVerified = false,
            EmailVerificationToken = "correct-token"
        };

        _userRepository.GetByEmailAsync(requestDto.Email).Returns(user);

        // Act
        var result = await _mockService.VerifyEmailAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsVerified.Should().BeFalse();
        result.Message.Should().Be("Invalid verification token");
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenTokenExpired_ReturnsExpiredRedirect()
    {
        // Arrange
        var requestDto = new EmailVerificationRequestDto
        {
            Email = "user@example.com",
            VerificationToken = "expired-token"
        };
        var frontendUrl = new Uri("http://localhost:4200");

        var user = new User
        {
            Id = 1,
            Email = requestDto.Email,
            IsVerified = false,
            EmailVerificationToken = "expired-token",
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(-1) // Expired
        };

        _userRepository.GetByEmailAsync(requestDto.Email).Returns(user);

        // Act
        var result = await _mockService.VerifyEmailAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsVerified.Should().BeFalse();
        result.Message.Should().Be("Verification token has expired");
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenValidToken_ReturnsSuccessRedirectAndUpdatesUser()
    {
        // Arrange
        var requestDto = new EmailVerificationRequestDto
        {
            Email = "user@example.com",
            VerificationToken = "valid-token"
        };
        var frontendUrl = new Uri("http://localhost:4200");

        var user = new User
        {
            Id = 1,
            Email = requestDto.Email,
            IsVerified = false,
            EmailVerificationToken = "valid-token",
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(1) // Not expired
        };

        _userRepository.GetByEmailAsync(requestDto.Email).Returns(user);

        // Act
        var result = await _mockService.VerifyEmailAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsVerified.Should().BeTrue();
        result.Message.Should().Be("Email verified successfully");

        // Verify user was updated
        await _userRepository.Received(1).UpdateUserAsync(Arg.Is<User>(u =>
            u.Id == user.Id &&
            u.IsVerified == true &&
            u.EmailVerificationToken == null &&
            u.EmailVerificationTokenExpiry == null));
    }

    #endregion

    #region ResendVerificationEmailAsync Tests

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestDto = new ResendVerificationRequestDto { Email = "nonexistent@example.com" };
        _userRepository.GetByEmailAsync(requestDto.Email).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mockService.ResendVerificationEmailAsync(requestDto));
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenUserAlreadyVerified_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestDto = new ResendVerificationRequestDto { Email = "verified@example.com" };
        var user = new User { Id = 1, Email = requestDto.Email, IsVerified = true };
        _userRepository.GetByEmailAsync(requestDto.Email).Returns(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mockService.ResendVerificationEmailAsync(requestDto));
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenValidUser_GeneratesNewTokenAndSendsEmail()
    {
        // Arrange
        var requestDto = new ResendVerificationRequestDto { Email = "user@example.com" };
        var user = new User
        {
            Id = 1,
            Email = requestDto.Email,
            FirstName = "John",
            IsVerified = false
        };
        _userRepository.GetByEmailAsync(requestDto.Email).Returns(user);

        // Act
        var result = await _mockService.ResendVerificationEmailAsync(requestDto);

        // Assert
        result.Should().BeTrue();
        await _userRepository.Received(1).UpdateUserAsync(Arg.Any<User>());
        await _emailSenderService.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
    #endregion
}
