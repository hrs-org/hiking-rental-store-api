using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Models;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API.Services;

public class AuthServiceTests
{
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly AuthService _service;
    private readonly IUserContextService _userContextService;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionService _userSessionService;
    private readonly IUserVerificationService _userVerificationService;

    public AuthServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userContextService = Substitute.For<IUserContextService>();
        _userSessionService = Substitute.For<IUserSessionService>();
        _userVerificationService = Substitute.For<IUserVerificationService>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _emailSenderService = Substitute.For<IEmailSenderService>();
        _service = new AuthService(_userRepository, _userContextService, _userSessionService, _userVerificationService, _emailBuilderService,
            _emailSenderService);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(new LoginRequestDto()));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordIncorrect()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@hrs.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("right"), IsVerified = true };
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        var dto = new LoginRequestDto { Email = user.Email, Password = "wrong" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenNotVerified()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@hrs.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"), IsVerified = false };
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        var dto = new LoginRequestDto { Email = user.Email, Password = "pass" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ReturnsResponse_WhenValid()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@hrs.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"), IsVerified = true };
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _userSessionService.CreateAsync(user.Id).Returns(("token", "refresh"));
        var dto = new LoginRequestDto { Email = user.Email, Password = "pass" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("token", result.Token);
        Assert.Equal("refresh", result.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsResponse()
    {
        // Arrange
        _userSessionService.RefreshAsync(Arg.Any<string>()).Returns(("token", "refresh"));
        _userContextService.GetUserAsync().Returns(new User { Id = 1 });

        // Act
        var result = await _service.RefreshTokenAsync("refresh");

        // Assert
        Assert.Equal(1, result.UserId);
        Assert.Equal("token", result.Token);
        Assert.Equal("refresh", result.RefreshToken);
    }

    [Fact]
    public async Task LogoutAsync_ReturnsLogoutResponse()
    {
        // Arrange
        _userContextService.GetUserAsync().Returns(new User { Id = 1 });
        _userSessionService.RevokeAllForUserAsync(1, Arg.Any<string>()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.LogoutAsync();

        // Assert
        Assert.Equal("Logout successful", result.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_ReturnsVerified_WhenValid()
    {
        // Arrange
        _userVerificationService.ValidateAndConsumeAsync(Arg.Any<string>(), "Email").Returns(new UserVerification { UserId = 1 });
        _userRepository.GetByIdAsync(1).Returns(new User());
        _userRepository.UpdateUserAsync(Arg.Any<User>()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.VerifyEmailAsync(new EmailVerificationRequestDto { VerificationToken = "token" });

        // Assert
        Assert.True(result.IsVerified);
    }

    [Fact]
    public async Task VerifyEmailAsync_ReturnsNotVerified_WhenInvalid()
    {
        // Arrange
        _userVerificationService.ValidateAndConsumeAsync(Arg.Any<string>(), "Email").Returns((UserVerification?)null);

        // Act
        var result = await _service.VerifyEmailAsync(new EmailVerificationRequestDto { VerificationToken = "token" });

        // Assert
        Assert.False(result.IsVerified);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ResendVerificationEmailAsync(new ResendVerificationRequestDto { Email = "notfound@hrs.com" }));
    }

    [Fact]
    public async Task ChangePasswordAsync_Throws_WhenCurrentPasswordIncorrect()
    {
        // Arrange
        _userContextService.GetUserAsync().Returns(new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("right") });
        var dto = new ChangePasswordRequestDto { CurrentPassword = "wrong", NewPassword = "new" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ChangePasswordAsync(dto));
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ReturnsTrue_WhenValid()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@hrs.com", FirstName = "Test", IsVerified = false };
        var verification = new UserVerification { Token = "token" };
        var requestDto = new ResendVerificationRequestDto { Email = "test@hrs.com" };
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _userVerificationService.CreateAsync(user.Id, "Email", Arg.Any<TimeSpan>()).Returns(verification);
        _emailBuilderService.BuildVerificationEmailTemplate(user.Email, verification.Token, user.FirstName).Returns(new EmailTemplate());
        _emailBuilderService.GenerateEmailBody(Arg.Any<EmailTemplate>()).Returns("body");
        _emailSenderService.SendEmailAsync(user.Email, Arg.Any<string>(), "body").Returns(true);

        // Act
        var result = await _service.ResendVerificationEmailAsync(requestDto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Throws_WhenAlreadyVerified()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@hrs.com", IsVerified = true };
        var requestDto = new ResendVerificationRequestDto { Email = "test@hrs.com" };
        _userRepository.GetByEmailAsync(user.Email).Returns(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResendVerificationEmailAsync(requestDto));
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsResponse_WhenValid()
    {
        // Arrange
        var user = new User { Id = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("old") };
        var requestDto = new ChangePasswordRequestDto { CurrentPassword = "old", NewPassword = "new" };
        _userContextService.GetUserAsync().Returns(user);
        _userRepository.UpdateUserAsync(user).Returns(Task.CompletedTask);

        // Act
        var result = await _service.ChangePasswordAsync(requestDto);

        // Assert
        Assert.Equal(user.Id, result.UserId);
        Assert.True(result.PasswordChangedAtUtc <= DateTime.UtcNow);
    }
}
