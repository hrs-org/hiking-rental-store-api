using HRS.API.Contracts.DTOs.Auth;

namespace HRS.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto requestDto);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto requestDto);
    Task<LogoutResponseDto> LogoutAsync();
    Task<EmailVerificationResponseDto> VerifyEmailAsync(EmailVerificationRequestDto requestDto);
    Task<bool> ResendVerificationEmailAsync(ResendVerificationRequestDto requestDto);
}
