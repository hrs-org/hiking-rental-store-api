using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
    {
        var res = await _authService.LoginAsync(dto);

        return Ok(ApiResponse<LoginResponseDto>.Ok(res, "Login successful"));
    }
}
