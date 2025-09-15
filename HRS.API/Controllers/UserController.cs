using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/user")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync() => Ok(await _userService.GetUsers());

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserAsync(int id)
    {
        var user = await _userService.GetUserById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        var res = await _userService.Register(dto);
        return Ok(ApiResponse<bool>.OkResponse(res, "Registration successful"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var success = await _userService.DeleteUser(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
