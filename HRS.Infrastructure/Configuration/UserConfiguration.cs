using HRS.Domain.Entities;
using HRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRS.Infrastructure.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .Property(u => u.Role)
            .HasConversion<string>();

        var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        builder.HasData(new User
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
    }
}
