using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using HRS.Domain.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace HRS.API.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IConfiguration config, IUserRepository userRepository, ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;

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
    public async Task LogoutAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) return;
        var keys = new[] { ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub, "nameid", "uid", "userId", "id" };
        int? userId = null;
        foreach (var k in keys)
        {
            var v = principal.FindFirstValue(k) ?? principal.Claims.FirstOrDefault(c => c.Type == k)?.Value;
            if (!string.IsNullOrWhiteSpace(v) && int.TryParse(v, out var id)) { userId = id; break; }
        }
        if (userId is null) return; 

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user is null) return;  

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepository.UpdateUserAsync(user);
    }
}
