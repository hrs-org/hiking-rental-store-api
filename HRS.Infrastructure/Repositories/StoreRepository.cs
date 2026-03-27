using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRS.Infrastructure.Repositories;

public class StoreRepository : CrudRepository<Store>, IStoreRepository
{
    public StoreRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<Store?> GetByUserIdAsync(int userId)
        => await _db.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<List<Store>> GetActiveStoresAsync()
        => await _db.Stores.Where(s => s.IsActive).ToListAsync();
}
