using AutoMapper;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Models;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API.Services;

public class UserServiceTests
{
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;
    private readonly IUserVerificationService _userVerificationService;

    public UserServiceTests()
    {
        _mapper = Substitute.For<IMapper>();
        _userRepository = Substitute.For<IUserRepository>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _emailSenderService = Substitute.For<IEmailSenderService>();
        _userContextService = Substitute.For<IUserContextService>();
        _userVerificationService = Substitute.For<IUserVerificationService>();
        _userService = new UserService(_mapper, _userRepository, _userContextService, _emailBuilderService, _emailSenderService, _userVerificationService);
    }

    [Fact]
    public async Task GetUsers_ReturnsMappedUserDtos()
    {
        // Arrange
        var users = new List<User> { new() { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com" } };
        var userDtos = new List<UserDto> { new() { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", Role = "Employee" } };
        _userRepository.GetAllAsync().Returns(users);
        _mapper.Map<IEnumerable<UserDto>>(users).Returns(userDtos);

        // Act
        var result = await _userService.GetUsers();

        // Assert
        Assert.Equal(userDtos, result);
    }

    [Fact]
    public async Task GetUserById_ReturnsMappedUserDto()
    {
        // Arrange
        var user = new User { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com" };
        var userDto = new UserDto { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", Role = "Employee" };
        _userRepository.GetByIdAsync(1).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _userService.GetUserById(1);

        // Assert
        Assert.Equal(userDto, result);
    }

    [Fact]
    public async Task DeleteUser_Throws_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.DeleteUser(1));
    }

    [Fact]
    public async Task DeleteUser_ReturnsTrue_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 1 };
        _userRepository.GetByIdAsync(1).Returns(user);

        // Act
        var result = await _userService.DeleteUser(1);

        // Assert
        Assert.True(result);
        _userRepository.Received(1).Remove(user);
    }

    [Fact]
    public async Task GetEmployees_ReturnsMappedEmployeeDtos()
    {
        // Arrange
        var user = new User { Id = 1, Role = UserRole.Admin };
        var employees = new List<User> { new() { Id = 2, Role = UserRole.Employee } };
        var employeeDtos = new List<UserDto>
        {
            new()
            {
                Id = 1,
                Role = "Employee",
                FirstName = "Test",
                LastName = "Name",
                Email = "testname@email.com"
            }
        };
        _userContextService.GetUserAsync().Returns(user);
        _userRepository.GetAllEmployee(true).Returns(employees);
        _mapper.Map<List<UserDto>>(employees).Returns(employeeDtos);

        // Act
        var result = await _userService.GetEmployees();

        // Assert
        Assert.Equal(employeeDtos, result);
    }

    [Fact]
    public async Task UpdateEmployee_Throws_WhenEmployeeNotFound()
    {
        // Arrange
        _userRepository.GetByIdAsync(1).Returns((User?)null);
        var dto = new UpdateEmployeeDto
        {
            Id = 1,
            Role = "Employee",
            FirstName = "Test",
            LastName = "Name",
            Email = "testname@email.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateEmployee(dto));
    }

    [Fact]
    public async Task UpdateEmployee_Throws_WhenRoleIsCustomer()
    {
        // Arrange
        var employee = new User { Id = 1, Role = UserRole.Customer };
        _userRepository.GetByIdAsync(1).Returns(employee);
        var dto = new UpdateEmployeeDto
        {
            Id = 1,
            Role = "Employee",
            FirstName = "Test",
            LastName = "Name",
            Email = "testname@email.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateEmployee(dto));
    }

    [Fact]
    public async Task UpdateEmployee_Throws_WhenRoleIsInvalid()
    {
        // Arrange
        var employee = new User { Id = 1, Role = UserRole.Employee };
        _userRepository.GetByIdAsync(1).Returns(employee);
        var dto = new UpdateEmployeeDto
        {
            Id = 1,
            Role = "Invalid Role",
            FirstName = "Test",
            LastName = "Name",
            Email = "testname@email.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateEmployee(dto));
    }

    [Fact]
    public async Task Register_ReturnsTrue_WhenValid()
    {
        // Arrange
        var dto = new RegisterDto { Email = "test@hrs.com", Password = "ValidPass123!", FirstName = "Test", LastName = "User" };
        var user = new User { Id = 1, Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName };
        var verification = new UserVerification { Token = "token" };
        var template = new EmailTemplate();
        _userRepository.GetByEmailAsync(dto.Email).Returns((User?)null);
        _mapper.Map<User>(dto).Returns(user);
        _userVerificationService.CreateAsync(user.Id, "Email", Arg.Any<TimeSpan>()).Returns(verification);
        _emailBuilderService.BuildVerificationEmailTemplate(user.Email, verification.Token, user.FirstName).Returns(template);
        _emailBuilderService.GenerateEmailBody(template).Returns("body");
        _emailSenderService.SendEmailAsync(user.Email, Arg.Any<string>(), "body").Returns(true);

        // Act
        var result = await _userService.Register(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Register_Throws_WhenUserExists()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@hrs.com",
            Password = "ValidPass123!",
            FirstName = "test",
            LastName = "name"
        };
        var existingUser = new User { Id = 1, Email = dto.Email };
        _userRepository.GetByEmailAsync(dto.Email).Returns(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.Register(dto));
    }

    [Fact]
    public async Task Register_Throws_WhenPasswordTooShort()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@hrs.com",
            Password = "short",
            FirstName = "test",
            LastName = "name"
        };
        _userRepository.GetByEmailAsync(dto.Email).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _userService.Register(dto));
    }

    [Fact]
    public async Task UpdateEmployee_UpdatesEmployee_WhenValid()
    {
        var editor = new User { Id = 99, Role = UserRole.Admin };
        var employee = new User { Id = 1, Role = UserRole.Employee };
        var dto = new UpdateEmployeeDto { Id = 1, Role = "Manager", FirstName = "F", LastName = "L", Email = "e@x.com" };
        var respond = new UserDto { Id = 1, Role = "Manager", FirstName = "F", LastName = "L", Email = "e@x.com" };
        _userContextService.GetUserAsync().Returns(editor);
        _userRepository.GetByIdAsync(dto.Id).Returns(employee);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(respond);

        var result = await _userService.UpdateEmployee(dto);

        Assert.Equal(dto.FirstName, employee.FirstName);
        Assert.Equal(dto.LastName, employee.LastName);
        Assert.Equal(dto.Email, employee.Email);
        Assert.Equal(UserRole.Manager, employee.Role);
        Assert.Equal(editor.Id, employee.UpdatedBy);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteEmployee_DeletesEmployee_WhenValid()
    {
        var employee = new User { Id = 2, Role = UserRole.Employee };
        _userRepository.GetByIdAsync(employee.Id).Returns(employee);

        var result = await _userService.DeleteEmployee(employee.Id);

        Assert.True(result);
        _userRepository.Received(1).Remove(employee);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteEmployee_Throws_WhenUserIsCustomer()
    {
        var employee = new User { Id = 3, Role = UserRole.Customer };
        _userRepository.GetByIdAsync(employee.Id).Returns(employee);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.DeleteEmployee(employee.Id));
    }

    [Fact]
    public async Task CreateNewEmployee_CreatesEmployeeAndReturnsDto()
    {
        var dto = new RegisterEmployeeDetailDto
        {
            FirstName = "New",
            LastName = "Employee",
            Email = "newemployee@mail.com",
            Role = "Employee"
        };
        var user = new User { Id = 4 };
        var editor = new User { Id = 99, Role = UserRole.Admin };
        var userDto = new UserDto
        {
            Id = 4,
            FirstName = "New",
            LastName = "Employee",
            Email = "newemployee@mail,com",
            Role = "Employee"
        };

        _mapper.Map<User>(dto).Returns(user);
        _userContextService.GetUserAsync().Returns(editor);
        _mapper.Map<UserDto>(user).Returns(userDto);

        var result = await _userService.CreateNewEmployee(dto);

        Assert.Equal(userDto, result);
        Assert.Equal(editor.Id, user.UpdatedBy);
        Assert.NotNull(user.PasswordHash);
        await _userRepository.Received(1).AddAsync(user);
        await _userRepository.Received(1).SaveChangesAsync();
    }
}
