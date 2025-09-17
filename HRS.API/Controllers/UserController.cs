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
        var createdUser = await _userService.Register(dto);
        return CreatedAtAction(nameof(GetUserAsync), new { id = createdUser.Id }, createdUser);
    }
    [HttpGet("employee")]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<List<RegisterEmployeeDetailDto>>> GetEmployees()
    {
        var employeeList = await _userService.GetEmployees();
        if (employeeList == null || employeeList.Count == 0) return NotFound();
        return Ok(employeeList);
    }

    [HttpPut("employee")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RegisterEmployeeDetailDto>> UpdateEmployee([FromBody] RegisterEmployeeDetailDto dto)
    {
        var updatedEmployee = await _userService.UpdateEmployee(dto);
        if (updatedEmployee == null) return NotFound();
        return Ok(updatedEmployee);
    }

    [HttpDelete("employee")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee([FromBody] RegisterEmployeeDetailDto dto)
    {
        var success = await _userService.DeleteEmployee(dto);
        if (!success) return NotFound();
        return NoContent();
    }
    [HttpPost("new-employee")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateNewEmployee([FromBody] RegisterEmployeeDetailDto dto)
    {
        var createdUser = await _userService.CreateNewEmployee(dto);
        return CreatedAtAction(nameof(GetUserAsync), new { id = createdUser.Id }, createdUser);
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
