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
using FluentAssertions;
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
        _mapper = Substitute.For<IMapper>();
        _userRepository = Substitute.For<IUserRepository>();
        _service = new UserService(_mapper, _userRepository);
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


        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);


        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = await _service.DeleteEmployee(dto);

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

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Admin, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);


        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Admin",
        };

        // Act
        var result = await _service.DeleteEmployee(dto);

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

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Customer, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);


        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = async () => await _service.DeleteEmployee(dto);


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

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Hey",
            LastName = "Girl",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = async () => await _service.DeleteEmployee(dto);

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

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);


        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Hey",
            LastName = "Girl",
            Email = " ",
            Role = "Employee",
        };

        // Act
        var result = async () => await _service.DeleteEmployee(dto);

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


        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = " ", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = " ",
            Role = "Manager",
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);

        // Act
        var result = await _service.UpdateEmployee(dto);

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
        // await userRepository.Received(1).GetByIdAsync(dto.Id);
        // await userRepository.Received(1).SaveChangesAsync();

    }

    [Fact]
    public async Task UpdateEmployee_True2()// Update Employee Detail
    {

        // Arrange

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));
        // Mock IMapper



        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Shen",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = await _service.UpdateEmployee(dto);

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
        // await userRepository.Received(1).GetByIdAsync(dto.Id);
        // await userRepository.Received(1).SaveChangesAsync();


    }

    [Fact]
    public async Task UpdateEmployee_False()// Customer cannot be update
    {

        // Arrange
        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Customer, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        // Mock IMapper


        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Shen",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = async () => await _service.UpdateEmployee(dto);

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
    public async Task UpdateEmployee_False2()// Don't have in database
    {

        // Arrange

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "XXX", Role = UserRole.Manager, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));



        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = async () => await _service.UpdateEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
        var updatedUser = await _userRepository.GetByIdAsync(dto.Id);
        Assert.Null(updatedUser);


    }
    [Fact]
    public async Task UpdateEmployee_True3()// No Update
    {

        // Arrange

        // Seed the in-memory database with a user
        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "XXX", Role = UserRole.Manager, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(2).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));

        var dto = new RegisterEmployeeDetailDto
        {
            Id = 2,
            FirstName = "Evan",
            LastName = "Feri",
            Email = "XXX",
            Role = "Manager",
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = await _service.UpdateEmployee(dto);

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
    public async Task UpdateEmployee_False3()// "update worng role"
    {

        // Arrange

        // Seed the in-memory database with a user
        var user = new User { Id = 3, FirstName = "Evan", LastName = "Jasper", Email = "III", Role = UserRole.Employee, PasswordHash = "123456" };
        _userRepository.GetByIdAsync(3).Returns(user);
        _userRepository.SaveChangesAsync().Returns(Task.FromResult(1));


        var dto = new RegisterEmployeeDetailDto
        {
            Id = 3,
            FirstName = "Evan",
            LastName = "Jasper",
            Email = "III",
            Role = "Teacher",
        };
        _mapper.Map<RegisterEmployeeDetailDto>(user).Returns(dto);
        // Act
        var result = async () => await _service.UpdateEmployee(dto);

        // Assert
        await result.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid role specified.");


    }




}


