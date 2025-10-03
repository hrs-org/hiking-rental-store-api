using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserContextService _userContextService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSenderService _emailSenderService;

    public AuthService(IUserRepository userRepository, IUserContextService userContextService, ITokenService tokenService, IEmailBuilderService emailBuilderService, IEmailSenderService emailSenderService)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _tokenService = tokenService;
        _emailBuilderService = emailBuilderService;
        _emailSenderService = emailSenderService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto requestDto)
    {
        var user = await _userRepository.GetByEmailAsync(requestDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(requestDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("email or password is incorrect");

        if (!user.IsVerified)
            throw new UnauthorizedAccessException("Please verify your email before logging in");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateUserAsync(user);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto requestDto)
    {
        var user = await _userContextService.GetUserAsync();

        if (user.RefreshToken != requestDto.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("invalid request token");

        // Generate new access token
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Update refresh token in DB
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateUserAsync(user);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<LogoutResponseDto> LogoutAsync()
    {
        var user = await _userContextService.GetUserAsync();

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepository.UpdateUserAsync(user);

        return new LogoutResponseDto { Message = "Logout successful" };
    }

    public async Task<EmailVerificationResponseDto> VerifyEmailAsync(EmailVerificationRequestDto requestDto)
    {
        var user = await _userRepository.GetByEmailAsync(requestDto.Email);
        if (user == null)
        {
            return new EmailVerificationResponseDto
            {
                IsVerified = false,
                Message = "User not found"
            };
        }

        if (user.IsVerified)
        {
            return new EmailVerificationResponseDto
            {
                IsVerified = true,
                Message = "Email is already verified"
            };
        }

        if (user.EmailVerificationToken != requestDto.VerificationToken)
        {
            return new EmailVerificationResponseDto
            {
                IsVerified = false,
                Message = "Invalid verification token"
            };
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return new EmailVerificationResponseDto
            {
                IsVerified = false,
                Message = "Verification token has expired"
            };
        }

        user.IsVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _userRepository.UpdateUserAsync(user);

        return new EmailVerificationResponseDto
        {
            IsVerified = true,
            Message = "Email verified successfully"
        };
    }

    public async Task<bool> ResendVerificationEmailAsync(ResendVerificationRequestDto requestDto)
    {
        var user = await _userRepository.GetByEmailAsync(requestDto.Email);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.IsVerified)
            throw new InvalidOperationException("Email is already verified");

        // Generate new verification token
        user.EmailVerificationToken = Guid.NewGuid().ToString();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _userRepository.UpdateUserAsync(user);

        // Send verification email using builder and sender directly
        var subject = "Verify Your Email - Hiking Rental Store";
        var emailTemplate = _emailBuilderService.BuildVerificationEmailTemplate(user.Email, user.EmailVerificationToken, user.FirstName);
        var body = _emailBuilderService.GenerateEmailBody(emailTemplate);
        await _emailSenderService.SendEmailAsync(user.Email, subject, body);

        return true;
    }

    public async Task<ChangePasswordResponseDto> ChangePasswordAsync(ChangePasswordRequestDto requestDto)
    {
        var user = await _userContextService.GetUserAsync();

        if (string.IsNullOrEmpty(requestDto.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(requestDto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.NewPassword);

        await _userRepository.UpdateUserAsync(user);

        return new ChangePasswordResponseDto
        {
            UserId = user.Id,
            PasswordChangedAtUtc = DateTime.UtcNow,
        };
    }
}
