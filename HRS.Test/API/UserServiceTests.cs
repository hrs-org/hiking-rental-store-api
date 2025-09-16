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

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        var context = new AppDbContext(options);

        // ใช้ Substitute.For<T>() แทน new Mock<T>()
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();

        _service = new UserService(context, _mapper, _userRepository);
    }

    // [Fact]
    // public async Task DeleteEmployee_TruePositive_ReturnsTrue()
    // {
    //     // Arrange
    //     var employee = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
    //     var data = new List<User> { employee }.AsQueryable();
    //     var mockSet = new Mock<DbSet<User>>();
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(data.Provider);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(data.Expression);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(data.ElementType);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    //     _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
    //     _mockContext.Setup(c => c.Users.Remove(It.IsAny<User>()));
    //     _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

    //     var dto = new RegisterEmployeeDetailDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com", Role = "Employee" };

    //     // Act
    //     var result = await _service.DeleteEmployee(dto);

    //     // Assert
    //     Assert.True(result);
    // }

    // [Fact]
    // public async Task DeleteEmployee_TrueNegative_ReturnsFalse()
    // {
    //     // Arrange
    //     var data = new List<User>().AsQueryable();
    //     var mockSet = new Mock<DbSet<User>>();
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(data.Provider);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(data.Expression);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(data.ElementType);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    //     _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

    //     var dto = new RegisterEmployeeDetailDto { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@smith.com", Role = "Employee" };

    //     // Act
    //     var result = await _service.DeleteEmployee(dto);

    //     // Assert
    //     Assert.False(result);
    // }

    // [Fact]
    // public async Task DeleteEmployee_FalsePositive_ReturnsTrueButNoEmployee()
    // {
    //     // Simulate method returns true even if employee does not exist (should not happen in real code)
    //     // Arrange
    //     var data = new List<User>().AsQueryable();
    //     var mockSet = new Mock<DbSet<User>>();
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(data.Provider);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(data.Expression);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(data.ElementType);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    //     _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
    //     // Force SaveChangesAsync to return 1 (simulate deletion)
    //     _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

    //     var dto = new RegisterEmployeeDetailDto { Id = 3, FirstName = "Ghost", LastName = "User", Email = "ghost@user.com", Role = "Employee" };

    //     // Act
    //     var result = await _service.DeleteEmployee(dto);

    //     // Assert
    //     // In real code, this should be false, but if method returns true, it's a false positive
    //     Assert.False(result); // Should be false, but if true, it's a bug
    // }

    // [Fact]
    // public async Task DeleteEmployee_FalseNegative_ReturnsFalseButEmployeeExists()
    // {
    //     // Simulate method returns false even if employee exists (should not happen in real code)
    //     // Arrange
    //     var employee = new User { Id = 4, FirstName = "Alice", LastName = "Wonder", Email = "alice@wonder.com" };
    //     var data = new List<User> { employee }.AsQueryable();
    //     var mockSet = new Mock<DbSet<User>>();
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(data.Provider);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(data.Expression);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(data.ElementType);
    //     mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    //     _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
    //     // Simulate SaveChangesAsync returns 0 (no changes saved)
    //     _mockContext.Setup(c => c.Users.Remove(It.IsAny<User>()));
    //     _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

    //     var dto = new RegisterEmployeeDetailDto { Id = 4, FirstName = "Alice", LastName = "Wonder", Email = "alice@wonder.com", Role = "Employee" };

    //     // Act
    //     var result = await _service.DeleteEmployee(dto);

    //     // Assert
    //     // In real code, this should be true, but if false, it's a false negative
    //     Assert.True(result); // Should be true, but if false, it's a bug
    // }
    [Fact]
    public async Task CreateEmployee_True()
    {
        // Arrange
        var dto = new RegisterEmployeeDetailDto { Id = 4, FirstName = "Alice", LastName = "Wonder", Email = "alice@wonder.com", Role = "Employee" };
        _mapper.Map<User>(dto).Returns(new User
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = Domain.Enums.UserRole.Employee
        });
        // Act
        var result = await _service.CreateNewEmployee(dto);

        // Assert

        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal("Employee", result.Role);
        // _mockUserSet.Verify(m => m.Add(It.IsAny<User>()), Times.Once);
        // _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_Flase()
    {
        // Arrange
        var dto = new RegisterEmployeeDetailDto { Id = 4, FirstName = "Alice", LastName = "Wonder", Email = "alice@wonder.com", Role = "Employee" };
        _mapper.Map<User>(dto).Returns(new User
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = Domain.Enums.UserRole.Employee
        });
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
 }

