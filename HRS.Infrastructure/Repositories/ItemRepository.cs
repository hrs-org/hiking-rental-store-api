using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRS.Infrastructure.Repositories;

public class ItemRepository : CrudRepository<Item>, IItemRepository
{
    public ItemRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<IEnumerable<Item>> GetRootItemsAsync()
    {
        return await _db.Items
            .Where(i => i.ParentId == null)
            .Include(i => i.Children)
            .Include(i => i.Rates)
            .ToListAsync();
    }

    public async Task<Item?> GetByIdWithChildrenAsync(int id)
    {
        return await _db.Items
            .Include(i => i.Children)
            .Include(i => i.Rates)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Item>> SearchAsync(string? keyword)
    {
        return await _db.Items
            .Where(i => string.IsNullOrWhiteSpace(keyword) || EF.Functions.Like(i.Name, $"%{keyword}%"))
            .ToListAsync();
    }
}
