using HRS.API.Contracts.DTOs.User;

namespace HRS.API.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsers();
    Task<UserDto> GetUserById(int id);
    Task<bool> Register(RegisterDto dto);
    Task<bool> DeleteUser(int id);
    Task<List<RegisterEmployeeDetailDto>> GetEmployees();
    Task<RegisterEmployeeDetailDto?> UpdateEmployee(RegisterEmployeeDetailDto dto);
    Task<bool> DeleteEmployee(RegisterEmployeeDetailDto dto);

    Task<UserDto> CreateNewEmployee(RegisterEmployeeDetailDto dto);



}
