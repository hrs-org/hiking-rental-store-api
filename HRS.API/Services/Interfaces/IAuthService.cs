using HRS.API.Contracts.DTOs.Auth;

namespace HRS.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto requestDto);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto requestDto);
}
