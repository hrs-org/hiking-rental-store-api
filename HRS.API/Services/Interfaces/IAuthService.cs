using HRS.API.Contracts.DTOs.Auth;

namespace HRS.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto requestDto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<LogoutResponseDto> LogoutAsync();
    Task<EmailVerificationResponseDto> VerifyEmailAsync(EmailVerificationRequestDto requestDto);
    Task<bool> ResendVerificationEmailAsync(ResendVerificationRequestDto requestDto);
    Task<ChangePasswordResponseDto> ChangePasswordAsync(ChangePasswordRequestDto requestDto);
    Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto requestDto);
    Task<string> ResetPasswordAsync(ResetPasswordRequestDto requestDto);
}
