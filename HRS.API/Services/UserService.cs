using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class UserService : IUserService
{
    // private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public UserService(IMapper mapper, IUserRepository userRepository)
    {
        // _context = context;
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
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }
            if (dto.Password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long.");
            }
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("Error checking for existing user: " + ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Error with provided data: " + ex.Message, ex);
        }
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        _userRepository.Remove(user);
        return true;
    }
}
