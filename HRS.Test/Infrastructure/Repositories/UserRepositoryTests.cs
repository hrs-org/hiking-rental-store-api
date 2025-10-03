using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Infrastructure;
using HRS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRS.Test.Infrastructure.Repositories;

public class UserRepositoryTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenExists()
    {
        var dbName = $"UserRepoDb_{nameof(GetByEmailAsync_ReturnsUser_WhenExists)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        var user = new User { Id = 2, FirstName = "Evan", LastName = "Feri", Email = "test@mail.com", Role = UserRole.Manager, PasswordHash = "123456" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByEmailAsync("test@mail.com");

        Assert.NotNull(result);
        Assert.Equal("test@mail.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
    {
        var dbName = $"UserRepoDb_{nameof(GetByEmailAsync_ReturnsNull_WhenNotFound)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        var result = await repository.GetByEmailAsync("notfound@mail.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesToken_WhenUserExists()
    {
        var dbName = $"UserRepoDb_{nameof(UpdateUserAsync_UpdatesToken_WhenUserExists)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        var user = new User { Id = 1, FirstName = "Evan", LastName = "Feri", Email = "test@mail.com", Role = UserRole.Manager, PasswordHash = "123456" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var update = new User
        {
            Id = 1,
            RefreshToken = "new-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        await repository.UpdateUserAsync(update);

        var updated = await dbContext.Users.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("new-token", updated!.RefreshToken);
        Assert.True(updated.RefreshTokenExpiry > DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateUserAsync_Throws_WhenUserNotFound()
    {
        var dbName = $"UserRepoDb_{nameof(UpdateUserAsync_Throws_WhenUserNotFound)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        var update = new User { Id = 999, RefreshToken = "xxx" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateUserAsync(update));
    }

    [Fact]
    public async Task GetAllEmployee_ReturnsOnlyEmployees()
    {
        var dbName = $"UserRepoDb_{nameof(GetAllEmployee_ReturnsOnlyEmployees)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        dbContext.Users.AddRange(
            new User { Id = 1, FirstName = "Evan", LastName = "Feri", Email = "test@mail.com", Role = UserRole.Employee, PasswordHash = "123456" },
            new User { Id = 2, FirstName = "Jaseper", LastName = "Shen", Email = "test2@mail.com", Role = UserRole.Manager, PasswordHash = "123456" }
        );
        await dbContext.SaveChangesAsync();

        var employees = await repository.GetAllEmployee();

        Assert.Single(employees);
        Assert.Equal(UserRole.Employee, employees.First().Role);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_ReturnsFalse_WhenEmailExists()
    {
        var dbName = $"UserRepoDb_{nameof(IsEmailUniqueAsync_ReturnsFalse_WhenEmailExists)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        dbContext.Users.Add(new User { Id = 1, FirstName = "Evan", LastName = "Feri", Email = "test@mail.com", Role = UserRole.Employee, PasswordHash = "123456" });
        await dbContext.SaveChangesAsync();

        var result = await repository.IsEmailUniqueAsync("test@mail.com");

        Assert.False(result);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_ReturnsTrue_WhenEmailNotExists()
    {
        var dbName = $"UserRepoDb_{nameof(IsEmailUniqueAsync_ReturnsTrue_WhenEmailNotExists)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        var result = await repository.IsEmailUniqueAsync("unique@mail.com");

        Assert.True(result);
    }

    [Fact]
    public async Task IsIdUniqueAsync_ReturnsFalse_WhenIdExists()
    {
        var dbName = $"UserRepoDb_{nameof(IsIdUniqueAsync_ReturnsFalse_WhenIdExists)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        dbContext.Users.Add(new User { Id = 3, FirstName = "Evan", LastName = "Feri", Email = "test@mail.com", Role = UserRole.Employee, PasswordHash = "123456" });
        await dbContext.SaveChangesAsync();

        var result = await repository.IsIdUniqueAsync(3);

        Assert.False(result);
    }

    [Fact]
    public async Task IsIdUniqueAsync_ReturnsTrue_WhenIdNotExists()
    {
        var dbName = $"UserRepoDb_{nameof(IsIdUniqueAsync_ReturnsTrue_WhenIdNotExists)}_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repository = new UserRepository(dbContext);

        var result = await repository.IsIdUniqueAsync(123);

        Assert.True(result);
    }
}
