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

    public AuthService(IUserRepository userRepository, IUserContextService userContextService, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto requestDto)
    {
        var user = await _userRepository.GetByEmailAsync(requestDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(requestDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("email or password is incorrect");

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
    public async Task<ChangePasswordResponseDto> ChangePasswordAsync(ChangePasswordRequestDto requestDto)
    {
        var user = await _userContextService.GetUserAsync();
        if (string.IsNullOrEmpty(requestDto.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(requestDto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");
        if (BCrypt.Net.BCrypt.Verify(requestDto.NewPassword, user.PasswordHash))
            throw new InvalidOperationException("New password must be different from the current password.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _userRepository.UpdateUserAsync(user);

        return new ChangePasswordResponseDto
        {
            UserId = user.Id,
            PasswordChangedAtUtc = DateTime.UtcNow,
            RefreshTokensRevoked = true
        };
    }
}
