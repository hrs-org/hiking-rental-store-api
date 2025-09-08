using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRS.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public UserService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> GetUserById(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> Register(RegisterDto dto)
    {
        var user = _mapper.Map<User>(dto);

        // TODO: hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
