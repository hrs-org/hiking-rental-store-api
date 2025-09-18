using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.Domain.Interfaces;
using HRS.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
// if you store UserRole here

namespace HRS.Test.API.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
// using Moq;
using Xunit;
using HRS.API.Services;
using HRS.API.Contracts.DTOs.User;
using HRS.Domain.Entities;
using HRS.Infrastructure;
using HRS.Infrastructure.Repositories;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _service;

    // private readonly ICrudRepository<User> _crudRepository;
    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        var context = new AppDbContext(options);

        // Use Substitute.For<T>() insteade new Mock<T>()
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();

        _service = new UserService(context, _mapper, _userRepository);
    }

    [Fact]
    public async Task CreateEmployee_True()//Normal
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
            Role = Domain.Enums.UserRole.Employee
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
        var result = await _service.CreateNewEmployee(dto);

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
            Role = Domain.Enums.UserRole.Employee
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
        var result = await _service.CreateNewEmployee(dto);

        // Assert

        Assert.NotNull(result);
        Assert.NotEqual("KRit", result.FirstName);
        Assert.NotEqual("Feri", result.LastName);
        Assert.NotEqual("Evan", result.Email);
        Assert.NotEqual("Jasper", result.Role);
        // _mockUserSet.Verify(m => m.Add(It.IsAny<User>()), Times.Once);
        // _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetAllEmployees_True() //Normal
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 69, FirstName = "Krit", LastName = "Handsome", Email = " ", Role = UserRole.Employee },
            new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee },
            new User { Id = 3, FirstName = "Feri", LastName = "Shen", Email = " ", Role = UserRole.Customer } // Not an employee
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
        var result = await _service.GetEmployees();
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
            new User { Id = 69, FirstName = "Krit", LastName = "Handsome", Email = " ", Role = UserRole.Customer },
            new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Customer },
            new User { Id = 3, FirstName = "IWant", LastName = "To", Email = " ", Role = UserRole.Customer },
            new User { Id = 3, FirstName = "Play", LastName = "Game", Email = "SoBadly ", Role = UserRole.Customer },
            new User { Id = 3, FirstName = "Feri", LastName = "Shen", Email = " ", Role = UserRole.Customer } // Not an employee
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
        var result = await _service.GetEmployees();
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
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteEmployeeDb")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);

        var service = new UserService(context, _mapper, _userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = await service.DeleteEmployee(dto);

        // Assert
        Assert.True(result);
        context.Dispose();
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.Received(1).Remove(user);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_True2() // Role is Admin
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteEmployeeDb2")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Admin, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);

        var service = new UserService(context, _mapper, _userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Admin",
        };

        // Act
        var result = await service.DeleteEmployee(dto);

        // Assert
        Assert.True(result);
        context.Dispose();
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.Received(1).Remove(user);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_False() //Wrong role
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteEmployeeDb3")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Customer, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var service = new UserService(context, _mapper, _userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = await service.DeleteEmployee(dto);

        // Assert
        Assert.False(result);
        context.Dispose();
        _userRepository.DidNotReceive().Remove(user);
        await _userRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_False2() //Wrong detail
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteEmployeeDb4")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var service = new UserService(context, _mapper, _userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Hey",
            LastName = "Girl",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = await service.DeleteEmployee(dto);

        // Assert
        Assert.False(result);
        context.Dispose();
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.DidNotReceive().Remove(user);
        await _userRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_False3() //Employee =null
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteEmployeeDb5")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var service = new UserService(context, _mapper, _userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 4,
            FirstName = "Hey",
            LastName = "Girl",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = await service.DeleteEmployee(dto);

        // Assert
        Assert.False(result);
        context.Dispose();
        await _userRepository.Received(1).GetByIdAsync(dto.Id);
        _userRepository.DidNotReceive().Remove(user);
        await _userRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateEmployee_True() // Update Role
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateEmployeeDb")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Mock IUserRepository (เรียก GetByIdAsync)
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(2).Returns(user);
        userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        // Mock IMapper
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RegisterEmployeeDetailDto>(Arg.Any<User>()).Returns(ci => new RegisterEmployeeDetailDto
        {
            Id = ((User)ci[0]).Id,
            FirstName = ((User)ci[0]).FirstName,
            LastName = ((User)ci[0]).LastName,
            Email = ((User)ci[0]).Email,
            Role = ((User)ci[0]).Role.ToString()
        });

        var service = new UserService(context, mapper, userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Manager",
        };

        // Act
        var result = await service.UpdateEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.Role, result.Role);
        Assert.NotEmpty(context.Users);


        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(dto.FirstName, updatedUser.FirstName);
        Assert.Equal(dto.LastName, updatedUser.LastName);
        Assert.Equal(dto.Email, updatedUser.Email);
        Assert.Equal(UserRole.Manager, updatedUser.Role);
        // await userRepository.Received(1).GetByIdAsync(dto.Id);
        // await userRepository.Received(1).SaveChangesAsync();
        context.Dispose();
    }

    [Fact]
    public async Task UpdateEmployee_True2()// Update Employee Detail
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateEmployeeDb2")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Employee, PasswordHash = "123456" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Mock IUserRepository (เรียก GetByIdAsync)
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(2).Returns(user);
        userRepository.SaveChangesAsync().Returns(Task.FromResult(1));


        // Mock IMapper
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RegisterEmployeeDetailDto>(Arg.Any<User>()).Returns(ci => new RegisterEmployeeDetailDto
        {
            Id = ((User)ci[0]).Id,
            FirstName = ((User)ci[0]).FirstName,
            LastName = ((User)ci[0]).LastName,
            Email = ((User)ci[0]).Email,
            Role = ((User)ci[0]).Role.ToString()
        });

        var service = new UserService(context, mapper, userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Shen",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };

        // Act
        var result = await service.UpdateEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Shen", result.FirstName);
        Assert.Equal("Feri", result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(result.Role, dto.Role);
        Assert.NotEmpty(context.Users);


        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(dto.FirstName, updatedUser.FirstName);
        Assert.Equal(dto.LastName, updatedUser.LastName);
        Assert.Equal(dto.Email, updatedUser.Email);
        Assert.Equal(UserRole.Manager, updatedUser.Role);
        // await userRepository.Received(1).GetByIdAsync(dto.Id);
        // await userRepository.Received(1).SaveChangesAsync();
        context.Dispose();

    }

    [Fact]
    public async Task UpdateEmployee_False()// Customer cannot be update
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateEmployeeDb3")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Customer, PasswordHash = "123456" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Mock IUserRepository (เรียก GetByIdAsync)
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(3).Returns(user);
        userRepository.SaveChangesAsync().Returns(Task.FromResult(1));


        // Mock IMapper
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RegisterEmployeeDetailDto>(Arg.Any<User>()).Returns(ci => new RegisterEmployeeDetailDto
        {
            Id = ((User)ci[0]).Id,
            FirstName = ((User)ci[0]).FirstName,
            LastName = ((User)ci[0]).LastName,
            Email = ((User)ci[0]).Email,
            Role = ((User)ci[0]).Role.ToString()
        });

        var service = new UserService(context, mapper, userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Shen",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };

        // Act
        var result = await service.UpdateEmployee(dto);

        // Assert
        Assert.Null(result);
        Assert.NotEmpty(context.Users);
        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        Assert.NotNull(updatedUser);
        Assert.NotEqual(dto.FirstName, updatedUser.FirstName);
        Assert.NotEqual(dto.LastName, updatedUser.LastName);
        Assert.NotEqual(dto.Email, updatedUser.Email);
        Assert.NotEqual(UserRole.Manager, updatedUser.Role);
        context.Dispose();

    }


    [Fact]
    public async Task UpdateEmployee_False2()// Don't have in database
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateEmployeeDb4")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "XXX", Role = UserRole.Manager, PasswordHash = "123456" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Mock IUserRepository (เรียก GetByIdAsync)
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(2).Returns(user);
        userRepository.SaveChangesAsync().Returns(Task.FromResult(1));


        // Mock IMapper
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RegisterEmployeeDetailDto>(Arg.Any<User>()).Returns(ci => new RegisterEmployeeDetailDto
        {
            Id = ((User)ci[0]).Id,
            FirstName = ((User)ci[0]).FirstName,
            LastName = ((User)ci[0]).LastName,
            Email = ((User)ci[0]).Email,
            Role = ((User)ci[0]).Role.ToString()
        });

        var service = new UserService(context, mapper, userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };

        // Act
        var result = await service.UpdateEmployee(dto);

        // Assert
        Assert.Null(result);
        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        Assert.Null(updatedUser);
        context.Dispose();

    }
    [Fact]
    public async Task UpdateEmployee_True3()// No Update
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateEmployeeDb5")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "XXX", Role = UserRole.Manager, PasswordHash = "123456" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Mock IUserRepository (เรียก GetByIdAsync)
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(2).Returns(user);
        userRepository.SaveChangesAsync().Returns(Task.FromResult(1));


        // Mock IMapper
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RegisterEmployeeDetailDto>(Arg.Any<User>()).Returns(ci => new RegisterEmployeeDetailDto
        {
            Id = ((User)ci[0]).Id,
            FirstName = ((User)ci[0]).FirstName,
            LastName = ((User)ci[0]).LastName,
            Email = ((User)ci[0]).Email,
            Role = ((User)ci[0]).Role.ToString()
        });

        var service = new UserService(context, mapper, userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };

        // Act
        var result = await service.UpdateEmployee(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.Role, result.Role);
        Assert.NotEmpty(context.Users);


        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(dto.FirstName, updatedUser.FirstName);
        Assert.Equal(dto.LastName, updatedUser.LastName);
        Assert.Equal(dto.Email, updatedUser.Email);
        Assert.Equal(UserRole.Manager, updatedUser.Role);
        context.Dispose();

    }

        [Fact]
    public async Task UpdateEmployee_False3()// "update worng role"
    {

        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateEmployeeDb6")
            .Options;

        await using var context = new AppDbContext(options);

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Employee, PasswordHash = "123456" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Mock IUserRepository (เรียก GetByIdAsync)
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(3).Returns(user);
        userRepository.SaveChangesAsync().Returns(Task.FromResult(1));


        // Mock IMapper
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RegisterEmployeeDetailDto>(Arg.Any<User>()).Returns(ci => new RegisterEmployeeDetailDto
        {
            Id = ((User)ci[0]).Id,
            FirstName = ((User)ci[0]).FirstName,
            LastName = ((User)ci[0]).LastName,
            Email = ((User)ci[0]).Email,
            Role = ((User)ci[0]).Role.ToString()
        });

        var service = new UserService(context, mapper, userRepository);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = "III",
            Role = "Teacher",
        };
        using var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        var result = await service.UpdateEmployee(dto);

        // Assert
        Assert.Null(result);
        Assert.NotEmpty(context.Users);
        var consoleOutput = sw.ToString().Trim(); // Get the console output
        Assert.Contains($"Warning: Invalid role '{dto.Role}'", consoleOutput);
        Assert.Contains(dto.Id.ToString(), consoleOutput);
        context.Dispose();

    }




}


