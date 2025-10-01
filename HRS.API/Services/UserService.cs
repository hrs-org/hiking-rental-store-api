using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;

    public UserService(IMapper mapper, IUserRepository userRepository, IUserContextService userContextService)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _userContextService = userContextService;
    }

    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> GetUserById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? throw new InvalidOperationException("User not found") : _mapper.Map<UserDto>(user);
    }

    public async Task<bool> Register(RegisterDto dto)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null) throw new InvalidOperationException("User with this email already exists.");
            if (dto.Password.Length < 8) throw new ArgumentException("Password must be at least 8 characters long.");
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
        if (user == null) throw new KeyNotFoundException("User not found.");
        _userRepository.Remove(user);
        return true;
    }

    public async Task<List<UserDto>> GetEmployees()
    {
        var user = await _userContextService.GetUserAsync();
        var employee = await _userRepository.GetAllEmployee(user.Role == UserRole.Admin);
        return _mapper.Map<List<UserDto>>(employee);
    }

    public async Task<UserDto?> UpdateEmployee(UserDto dto)
    {
        var editor = await _userContextService.GetUserAsync();
        var employee = await _userRepository.GetByIdAsync(dto.Id);
        if (employee == null) throw new KeyNotFoundException("User not found.");
        if (employee.Role == UserRole.Customer) throw new InvalidOperationException("Cannot update a customer to an employee.");

        if (dto.Role == "Employee" || dto.Role == "Manager" || dto.Role == "Admin")
        {
            var role = Enum.Parse<UserRole>(dto.Role);
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Role = role;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = editor.Id;
        }
        else
        {
            throw new ArgumentException("Invalid role specified.");
        }

        await _userRepository.SaveChangesAsync();
        return _mapper.Map<UserDto>(employee);
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        var employee = await _userRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("User not found.");
        if (employee.Role == UserRole.Customer) throw new InvalidOperationException("Cannot delete a customer as an employee.");

        _userRepository.Remove(employee);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto> CreateNewEmployee(RegisterEmployeeDetailDto dto)
    {
        var user = _mapper.Map<User>(dto);
        var editor = _userContextService.GetUserAsync();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = editor.Id;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123");

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        //Send email to user with password setup link
        return _mapper.Map<UserDto>(user);
    }
}
