using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Domain.Enums;

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
    public async Task<List<RegisterEmployeeDetailDto>> GetEmployees()
    {
        var employee = await _userRepository.GetAllEmployee();
        return _mapper.Map<List<RegisterEmployeeDetailDto>>(employee);
    }

    public async Task<RegisterEmployeeDetailDto?> UpdateEmployee(RegisterEmployeeDetailDto dto)
    {

        // if (dto.Role == "Customer" ) return null;
        var employee = await _userRepository.GetByIdAsync(dto.Id);
        if (employee == null) throw new KeyNotFoundException("User not found.");
        if (employee.Role == Domain.Enums.UserRole.Customer) throw new InvalidOperationException("Cannot update a customer to an employee.");

        if (dto.Role == "Employee" || dto.Role == "Manager" || dto.Role == "Admin")
        {
            var role = Enum.Parse<UserRole>(dto.Role);
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Role = role;

        }
        else
        {
            throw new ArgumentException("Invalid role specified.");
            // handle invalid role
        }
        // employee.Role = Enum.Parse<UserRole>(dto.Role);
        // _context.Users.Update(employee);
        await _userRepository.SaveChangesAsync();
        return _mapper.Map<RegisterEmployeeDetailDto>(employee);
    }
    public async Task<bool> DeleteEmployee(RegisterEmployeeDetailDto dto)
    {
        var employee = await _userRepository.GetByIdAsync(dto.Id);
        if (employee == null) throw new KeyNotFoundException("User not found.");
        if (employee.Role == Domain.Enums.UserRole.Customer)   throw new InvalidOperationException("Cannot delete a customer as an employee.");
        if (employee.FirstName != dto.FirstName || employee.LastName != dto.LastName || employee.Email != dto.Email) throw new InvalidOperationException("Employee details do not match.");
       _userRepository.Remove(employee);
        await _userRepository.SaveChangesAsync();
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
