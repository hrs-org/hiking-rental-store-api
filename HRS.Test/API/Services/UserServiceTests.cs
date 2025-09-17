using System.Diagnostics;
using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API;
public class UserServiceTests
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

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
            PasswordHash = dto.Password,
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
            PasswordHash = dto.Password,
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
            PasswordHash = dto.Password,
        };

        _userRepository.GetByEmailAsync(dto.Email).Returns((User?)null);
        _mapper.Map<User>(dto).Returns(user);

        // Act
        var result = async () => await _userService.Register(dto);

        // Assert
        await result.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*password*");
    }
}
