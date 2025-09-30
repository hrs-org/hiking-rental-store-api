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
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IUserContextService userContextService, IConfiguration configuration)
    {
        _authService = authService;
        _userContextService = userContextService;
        _configuration = configuration;
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
    
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmailAsync([FromQuery] string email, [FromQuery] string token)
    {
        var requestDto = new EmailVerificationRequestDto
        {
            Email = email,
            VerificationToken = token
        };

        var frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:4200";
        var frontendUri = new Uri(frontendUrl);
        var res = await _authService.VerifyEmailAsync(requestDto, frontendUri);
        
        return Redirect(res.RedirectUrl.ToString());
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerificationAsync([FromBody] ResendVerificationRequestDto requestDto)
    {
        var res = await _authService.ResendVerificationEmailAsync(requestDto);
        return Ok(ApiResponse<bool>.OkResponse(res, "Verification email sent successfully"));
    }
}
