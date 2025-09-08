using HRS.API.Contracts.DTOs.Auth;

namespace HRS.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
}
