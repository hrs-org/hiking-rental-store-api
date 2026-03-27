using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IStoreRepository : ICrudRepository<Store>
{
    Task<Store?> GetByUserIdAsync(int userId);
    Task<List<Store>> GetActiveStoresAsync();
}
