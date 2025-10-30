using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IPackageRateRepository : ICrudRepository<PackageRate>
{
    Task<IEnumerable<PackageRate>> GetRatesByPackageIdAsync(int packageId, bool activeOnly = true);
    Task<PackageRate?> GetApplicableRateAsync(int packageId, int rentalDays);
}
