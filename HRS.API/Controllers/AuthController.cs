using FluentValidation;
using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserContextService _userContextService;

    public AuthController(IAuthService authService, IUserContextService userContextService)
    {
        _authService = authService;
        _userContextService = userContextService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto requestDto, IValidator<LoginRequestDto> validator)
    {
        var res = await _authService.LoginAsync(requestDto);
        return Ok(ApiResponse<LoginResponseDto>.OkResponse(res, "Login successful"));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDto requestDto, IValidator<RefreshTokenRequestDto> validator)
    {
        var res = await _authService.RefreshTokenAsync(requestDto);
        return Ok(ApiResponse<LoginResponseDto>.OkResponse(res, "Refresh token successful"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        var res = await _authService.LogoutAsync();
        return Ok(ApiResponse<LogoutResponseDto>.OkResponse(res, "logout successful"));
    }
    
    [HttpGet("active-user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var res = await _userContextService.GetUserDtoAsync();
        return Ok(ApiResponse<UserDto>.OkResponse(res, "Get current user successful"));
    }
    
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync(
    [FromBody] ChangePasswordRequestDto requestDto)
    {
        var res = await _authService.ChangePasswordAsync(requestDto);
        return Ok(ApiResponse<ChangePasswordResponseDto>.OkResponse(res, "Password changed successfully"));
    }
}
