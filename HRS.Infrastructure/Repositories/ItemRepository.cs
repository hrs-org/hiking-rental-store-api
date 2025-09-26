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
            .ToListAsync();
    }

    public async Task<Item?> GetByIdWithChildrenAsync(int id)
    {
        return await _db.Items
            .Include(i => i.Children)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
}
