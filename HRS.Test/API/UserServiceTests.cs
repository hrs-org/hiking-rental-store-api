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

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _service;

    private readonly ICrudRepository<User> _crudRepository;
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
    public async Task CreateEmployee_True()
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
        Assert.Equal("Employee", result.Role);

        await _userRepository.Received(1).AddAsync(Arg.Any<User>());
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateEmployee_Flase()
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

        Assert.NotNull(result); //not sure how to assert false here
        Assert.NotEqual("KRit", result.FirstName);
        Assert.NotEqual("Feri", result.LastName);
        Assert.NotEqual("Evan", result.Email);
        Assert.NotEqual("Jasper", result.Role);
        // _mockUserSet.Verify(m => m.Add(It.IsAny<User>()), Times.Once);
        // _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }



    [Fact]
    public async Task GetAllEmployees_True()
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
    public async Task GetAllEmployees_False()
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
}


