using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public AuthService(IConfiguration config, IUserRepository userRepository, ITokenService tokenService)
    {
        _config = config;
        _userRepository = userRepository;
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
        var user = await _userRepository.GetByIdAsync(requestDto.UserId);

        if (user == null || user.RefreshToken != requestDto.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
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
}
