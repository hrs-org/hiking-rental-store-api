using HRS.Domain.Entities;
using HRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
        
        var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@hrs.com",
            PasswordHash = adminPassword,
            IsVerified = true,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        base.OnModelCreating(modelBuilder);
    }
}