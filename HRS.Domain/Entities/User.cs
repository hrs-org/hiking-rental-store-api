using HRS.Domain.Enums;

namespace HRS.Domain.Entities;

public class User
{
    public int Id { get; set; }  
    
    // New fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool IsVerified { get; set; }
    
    public UserRole Role { get; set; } = UserRole.Customer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}