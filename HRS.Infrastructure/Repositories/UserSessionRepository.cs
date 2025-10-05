using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRS.Infrastructure.Repositories;

public class UserSessionRepository : CrudRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(AppDbContext db) : base(db)
    {
    }

    public Task<List<UserSession>> RefreshSessionCandidateAsync()
    {
        return _db.UserSessions
            .Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
    }

    public Task<List<UserSession>> GetRevokedSessionsAsync()
    {
        return _db.UserSessions
            .Where(s => s.IsRevoked)
            .ToListAsync();
    }

    public Task RevokeAllSessionsAsync(int userId, string reason)
    {
        return _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ExecuteUpdateAsync(u => u
                .SetProperty(s => s.IsRevoked, true)
                .SetProperty(s => s.RevokedReason, reason));
    }

    public Task<int> CleanUpExpiredSessionsAsync()
    {
        var result = _db.UserSessions
            .Where(s => s.IsRevoked || s.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync();
        return result;
    }
}
