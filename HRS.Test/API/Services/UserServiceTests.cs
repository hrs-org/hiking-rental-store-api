using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API.Services;

public class UserServiceTests
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _mapper = Substitute.For<IMapper>();
        _userRepository = Substitute.For<IUserRepository>();
        _userService = new UserService(_mapper, _userRepository);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldRegisterUser()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@hrs.com",
            Password = "ValidPass123!"
        };

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = dto.Password
        };
        _userRepository.GetByEmailAsync(dto.Email).Returns((User?)null);
        _mapper.Map<User>(dto).Returns(user);

        // Act
        var result = await _userService.Register(dto);

        // Assert
        result.Should().BeTrue();
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetUsers_ReturnsMappedUserDtos()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com" },
            new() { Id = 2, FirstName = "C", LastName = "D", Email = "c@d.com" }
        };
        var userDtos = new List<UserDto>
        {
            new() { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", Role = "Employee" },
            new() { Id = 2, FirstName = "C", LastName = "D", Email = "c@d.com", Role = "Manager" }
        };
        _userRepository.GetAllAsync().Returns(users);
        _mapper.Map<IEnumerable<UserDto>>(users).Returns(userDtos);

        // Act
        var result = await _userService.GetUsers();

        // Assert
        result.Should().BeEquivalentTo(userDtos);
        await _userRepository.Received(1).GetAllAsync();
        _mapper.Received(1).Map<IEnumerable<UserDto>>(users);
    }

    [Fact]
    public async Task GetUserById_UserExists_ReturnsMappedUserDto()
    {
        // Arrange
        var user = new User { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com" };
        var userDto = new UserDto { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", Role = "Employee" };
        _userRepository.GetByIdAsync(1).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _userService.GetUserById(1);

        // Assert
        result.Should().Be(userDto);
        await _userRepository.Received(1).GetByIdAsync(1);
        _mapper.Received(1).Map<UserDto>(user);
    }

    [Fact]
    public async Task GetUserById_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        _userRepository.GetByIdAsync(99).Returns((User?)null);

        // Act
        var act = async () => await _userService.GetUserById(99);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User not found");
        await _userRepository.Received(1).GetByIdAsync(99);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "exist@hrs.com",
            Password = "ValidPass123!"
        };

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = dto.Password
        };

        _userRepository.GetByEmailAsync(dto.Email).Returns(new User { Email = dto.Email });
        _mapper.Map<User>(dto).Returns(user);

        // Act
        var result = async () => await _userService.Register(dto);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test2@hrs.com",
            Password = "123"
        };

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = dto.Password
        };

        _userRepository.GetByEmailAsync(dto.Email).Returns((User?)null);
        _mapper.Map<User>(dto).Returns(user);

        // Act
        var result = async () => await _userService.Register(dto);

        // Assert
        await result.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*password*");
    }

    [Fact]
    public async Task CreateEmployee_True() //Normal
    {
        // Arrange
        var dto = new RegisterEmployeeDetailDto
        {
            Id = 4,
            FirstName = "Alice",
            LastName = "Wonder",
            Email = "alice@wonder.com",
            Role = "Employee"
        };

        var user = new User
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = UserRole.Employee
        };

        var userDto = new UserDto
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = dto.Role
        };

        _mapper.Map<User>(dto).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);
        _userRepository.AddAsync(user).Returns(Task.CompletedTask);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        // Act
        var result = await _userService.CreateNewEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.Role, result.Role);

        await _userRepository.Received(1).AddAsync(Arg.Any<User>());
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateEmployee_Flase() // Detail not go wrong
    {
        // Arrange
        var dto = new RegisterEmployeeDetailDto { Id = 4, FirstName = "Alice", LastName = "Wonder", Email = "alice@wonder.com", Role = "Employee" };
        var user = new User
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = UserRole.Employee
        };

        var userDto = new UserDto
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = dto.Role
        };

        _mapper.Map<User>(dto).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);
        _userRepository.AddAsync(user).Returns(Task.CompletedTask);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        // Act
        var result = await _userService.CreateNewEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("KRit", result.FirstName);
        Assert.NotEqual("Feri", result.LastName);
        Assert.NotEqual("Evan", result.Email);
        Assert.NotEqual("Jasper", result.Role);
    }

    [Fact]
    public async Task GetAllEmployees_True() //Normal
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 69, FirstName = "Krit", LastName = "Handsome", Email = " ", Role = UserRole.Employee },
            new() { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee },
            new() { Id = 3, FirstName = "Feri", LastName = "Shen", Email = " ", Role = UserRole.Customer } // Not an employee
        }.AsQueryable();

        _userRepository.GetAllEmployee().Returns(users.Where(u => u.Role == UserRole.Employee).ToList());
        var employeeDtos = users
            .Where(u => u.Role == UserRole.Employee)
            .Select(u => new RegisterEmployeeDetailDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role.ToString()
            }).ToList();
        _mapper.Map<List<RegisterEmployeeDetailDto>>(Arg.Any<List<User>>()).Returns(employeeDtos);
        _mapper.Map<List<RegisterEmployeeDetailDto>>(Arg.Any<IEnumerable<User>>()).Returns(employeeDtos);

        // Act
        var result = await _userService.GetEmployees();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count); // Only 2 employees
        Assert.All(result, r => Assert.Equal("Employee", r.Role));
        await _userRepository.Received(1).GetAllEmployee();
    }

    [Fact]
    public async Task GetAllEmployees_False() // No Employee
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 69, FirstName = "Krit", LastName = "Handsome", Email = " ", Role = UserRole.Customer },
            new() { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Customer },
            new() { Id = 3, FirstName = "IWant", LastName = "To", Email = " ", Role = UserRole.Customer },
            new() { Id = 3, FirstName = "Play", LastName = "Game", Email = "SoBadly ", Role = UserRole.Customer },
            new() { Id = 3, FirstName = "Feri", LastName = "Shen", Email = " ", Role = UserRole.Customer } // Not an employee
        }.AsQueryable();

        _userRepository.GetAllEmployee().Returns(users.Where(u => u.Role == UserRole.Employee).ToList());
        var employeeDtos = users
            .Where(u => u.Role == UserRole.Employee)
            .Select(u => new RegisterEmployeeDetailDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role.ToString()
            }).ToList();
        _mapper.Map<List<RegisterEmployeeDetailDto>>(Arg.Any<List<User>>()).Returns(employeeDtos);
        _mapper.Map<List<RegisterEmployeeDetailDto>>(Arg.Any<IEnumerable<User>>()).Returns(employeeDtos);

        // Act
        var result = await _userService.GetEmployees();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.NotEqual(1, result.Count); // 0 employees
        Assert.DoesNotContain(result, r => r.Role == "Employee");
        await _userRepository.Received(1).GetAllEmployee();
    }

    [Fact]
    public async Task DeleteEmployee_True() // Role is Employee
    {
        // Arrange
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Employee"
        };

        // Act
        var result = await _userService.DeleteEmployee(dto);

        // Assert
        Assert.True(result);
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.Received(1).Remove(user);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_True2() // Role is Admin
    {
        // Arrange
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Admin, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Admin"
        };

        // Act
        var result = await _userService.DeleteEmployee(dto);

        // Assert
        Assert.True(result);
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.Received(1).Remove(user);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_False() //Wrong role (taget is customer)
    {
        // Arrange
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Customer, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Employee"
        };

        // Act
        var result = async () => await _userService.DeleteEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete a customer as an employee.");
        _userRepository.DidNotReceive().Remove(user);
        await _userRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_False2() //Wrong detail
    {
        // Arrange
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Hey",
            LastName = "Girl",
            Email = " ",
            Role = "Employee"
        };

        // Act
        var result = async () => await _userService.DeleteEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee details do not match.");
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.DidNotReceive().Remove(user);
        await _userRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_False3() //Employee =null
    {
        // Arrange
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Hey",
            LastName = "Girl",
            Email = " ",
            Role = "Employee"
        };

        // Act
        var result = async () => await _userService.DeleteEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.DidNotReceive().Remove(user);
        await _userRepository.DidNotReceive().SaveChangesAsync();
    }


    [Fact]
    public async Task UpdateEmployee_True() // Update Role
    {
        // Arrange
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Manager"
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);

        // Act
        var result = await _userService.UpdateEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.Role, result.Role);

        var updatedUser = await _userRepository.GetByIdAsync(dto.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(dto.FirstName, updatedUser.FirstName);
        Assert.Equal(dto.LastName, updatedUser.LastName);
        Assert.Equal(dto.Email, updatedUser.Email);
        Assert.Equal(UserRole.Manager, updatedUser.Role);
    }

    [Fact]
    public async Task UpdateEmployee_True2() // Update Employee Detail
    {
        // Arrange
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Shen",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager"
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = await _userService.UpdateEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Shen", result.FirstName);
        Assert.Equal("Feri", result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(result.Role, dto.Role);

        var updatedUser = await _userRepository.GetByIdAsync(dto.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(dto.FirstName, updatedUser.FirstName);
        Assert.Equal(dto.LastName, updatedUser.LastName);
        Assert.Equal(dto.Email, updatedUser.Email);
        Assert.Equal(UserRole.Manager, updatedUser.Role);
    }

    [Fact]
    public async Task UpdateEmployee_False() // Customer cannot be update
    {
        // Arrange
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Customer, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Shen",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager"
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);

        // Act
        var result = async () => await _userService.UpdateEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update a customer to an employee.");
        var updatedUser = await _userRepository.GetByIdAsync(dto.Id);
        Assert.NotNull(updatedUser);
        Assert.NotEqual(dto.FirstName, updatedUser.FirstName);
        Assert.NotEqual(dto.LastName, updatedUser.LastName);
        Assert.NotEqual(dto.Email, updatedUser.Email);
        Assert.NotEqual(UserRole.Manager, updatedUser.Role);
    }


    [Fact]
    public async Task UpdateEmployee_False2() // Don't have in database
    {
        // Arrange
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "XXX", Role = UserRole.Manager, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager"
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);

        // Act
        var result = async () => await _userService.UpdateEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
        var updatedUser = await _userRepository.GetByIdAsync(dto.Id);
        Assert.Null(updatedUser);
    }

    [Fact]
    public async Task UpdateEmployee_True3() // No Update
    {
        // Arrange
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "XXX", Role = UserRole.Manager, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager"
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);

        // Act
        var result = await _userService.UpdateEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.Role, result.Role);

        var updatedUser = await _userRepository.GetByIdAsync(dto.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(dto.FirstName, updatedUser.FirstName);
        Assert.Equal(dto.LastName, updatedUser.LastName);
        Assert.Equal(dto.Email, updatedUser.Email);
        Assert.Equal(UserRole.Manager, updatedUser.Role);
    }

    [Fact]
    public async Task UpdateEmployee_False3() // "update worng role"
    {
        // Arrange
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = "III",
            Role = "Teacher"
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = async () => await _userService.UpdateEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid role specified.");
    }
}
