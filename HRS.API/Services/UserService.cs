using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HRS.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public UserService(AppDbContext context, IMapper mapper, IUserRepository userRepository)
    {
        _context = context;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> GetUserById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> Register(RegisterDto dto)
    {
        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error checking for existing user: " + ex.Message, ex);
        }
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
