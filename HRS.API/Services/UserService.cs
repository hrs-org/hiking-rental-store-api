using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Infrastructure;
using Microsoft.EntityFrameworkCore;
using HRS.Domain.Enums;

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
    public async Task<List<RegisterEmployeeDetailDto>> GetEmployees()
    {
        var employee = await _userRepository.GetAllEmployee();
        return _mapper.Map<List<RegisterEmployeeDetailDto>>(employee);
    }

    public async Task<RegisterEmployeeDetailDto?> UpdateEmployee(RegisterEmployeeDetailDto dto)
    {
        // if (dto.Role == "Customer" ) return null;
        var employee = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (employee == null) return null;
        if (employee.Role == Domain.Enums.UserRole.Customer) return null;
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        if (Enum.TryParse<UserRole>(dto.Role, out var role))
        {
            employee.Role = role;
        }
        else
        {
            Console.WriteLine($"Warning: Invalid role '{dto.Role}' for user {dto.Id}");
            // handle invalid role, เช่น log หรือ throw exception
        }
        // employee.Role = Enum.Parse<UserRole>(dto.Role);
        // _context.Users.Update(employee);
        await _context.SaveChangesAsync();
        return _mapper.Map<RegisterEmployeeDetailDto>(employee);
    }
    public async Task<bool> DeleteEmployee(RegisterEmployeeDetailDto dto)
    {
        if (dto.Role == "Customer" ) return false;
        var employee = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (employee == null) return false;
        if (employee.Role == Domain.Enums.UserRole.Customer) return false;
        if (employee.FirstName != dto.FirstName || employee.LastName != dto.LastName || employee.Email != dto.Email) return false;
        _context.Users.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto> CreateNewEmployee(RegisterEmployeeDetailDto dto)
    {
        var user = _mapper.Map<User>(dto);

        // TODO: hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
        user.Role = Domain.Enums.UserRole.Employee;

        await _userRepository.AddAsync(user);
        // _context.Users.Add(user);
        await _userRepository.SaveChangesAsync();
        // await _context.SaveChangesAsync();
        //Send email to user with password setup link
        return _mapper.Map<UserDto>(user);
        // var user = await _userRepository.GetByIdAsync(id);
        // if (user == null) return false;
        // user.Role = Domain.Enums.UserRole.Employee;
        // await _context.SaveChangesAsync();
        // return true;

    }
}
