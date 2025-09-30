using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserContextService _userContextService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public AuthService(IUserRepository userRepository, IUserContextService userContextService, ITokenService tokenService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _tokenService = tokenService;
        _emailService = emailService;
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

    public async Task<EmailVerificationRedirectDto> VerifyEmailAsync(EmailVerificationRequestDto requestDto, Uri frontendUrl)
    {
        var user = await _userRepository.GetByEmailAsync(requestDto.Email);
        if (user == null)
        {
            var redirectUrl = new Uri(frontendUrl, $"/verification-failed?email={Uri.EscapeDataString(requestDto.Email)}&message={Uri.EscapeDataString("User not found")}");
            return new EmailVerificationRedirectDto
            {
                RedirectUrl = redirectUrl,
                IsVerified = false,
                Message = "User not found"
            };
        }

        if (user.IsVerified)
        {
            var redirectUrl = new Uri(frontendUrl, $"/verification-success?email={Uri.EscapeDataString(requestDto.Email)}");
            return new EmailVerificationRedirectDto
            {
                RedirectUrl = redirectUrl,
                IsVerified = true,
                Message = "Email is already verified"
            };
        }

        if (user.EmailVerificationToken != requestDto.VerificationToken)
        {
            var redirectUrl = new Uri(frontendUrl, $"/verification-failed?email={Uri.EscapeDataString(requestDto.Email)}&message={Uri.EscapeDataString("Invalid verification token")}");
            return new EmailVerificationRedirectDto
            {
                RedirectUrl = redirectUrl,
                IsVerified = false,
                Message = "Invalid verification token"
            };
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            var redirectUrl = new Uri(frontendUrl, $"/verification-expired?email={Uri.EscapeDataString(requestDto.Email)}");
            return new EmailVerificationRedirectDto
            {
                RedirectUrl = redirectUrl,
                IsVerified = false,
                Message = "Verification token has expired"
            };
        }

        user.IsVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _userRepository.UpdateUserAsync(user);

        var successRedirectUrl = new Uri(frontendUrl, $"/verification-success?email={Uri.EscapeDataString(requestDto.Email)}");
        return new EmailVerificationRedirectDto
        {
            RedirectUrl = successRedirectUrl,
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

        // Send verification email
        await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, user.FirstName);

        return true;
    }
}
