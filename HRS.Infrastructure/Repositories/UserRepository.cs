using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRS.Infrastructure.Repositories;

public class UserRepository : CrudRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task UpdateUserAsync(User user)
    {
        var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        if (dbUser == null)
            throw new KeyNotFoundException($"User with Id {user.Id} not found");

        dbUser.RefreshToken = user.RefreshToken;
        dbUser.RefreshTokenExpiry = user.RefreshTokenExpiry;
        dbUser.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
    public async Task<List<User>> GetAllEmployee()
        => await _db.Users.Where(u => u.Role == Domain.Enums.UserRole.Employee).ToListAsync();
}
