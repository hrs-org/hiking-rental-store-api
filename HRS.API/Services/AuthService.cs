using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HRS.API.Contracts.DTOs.Auth;
using HRS.API.Services.Interfaces;
using HRS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HRS.API.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("email or password is incorrect");

        // Generate JWT
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: cred
        );

        return new LoginResponseDto
        {
            UserId = user.Id,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
