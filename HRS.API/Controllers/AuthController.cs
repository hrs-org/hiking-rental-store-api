using FluentValidation;
using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
        await _authService.LogoutAsync();
        return Ok(new { success = true, message = "Logout successful" });
    }
}
