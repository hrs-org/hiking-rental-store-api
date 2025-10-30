using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IPaymentRepository : ICrudRepository<Payment>
{
    Task<Payment?> GetByRentalOrderIdAsync(int rentalOrderId);
    Task<List<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
