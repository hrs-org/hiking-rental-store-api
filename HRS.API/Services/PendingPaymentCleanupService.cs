using System;
using System.Linq;
using System.Threading.Tasks;
using HRS.API.Services.Interfaces;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRS.API.Services;

public class PendingPaymentCleanupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingPaymentCleanupService> _logger;
    private readonly TimeSpan _pendingTimeout = TimeSpan.FromMinutes(1);

    public PendingPaymentCleanupService(IServiceProvider serviceProvider, ILogger<PendingPaymentCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task CleanupPendingPayments()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var rentalOrderRepository = scope.ServiceProvider.GetRequiredService<IRentalOrderRepository>();
            var now = DateTime.UtcNow;
            var orders = await rentalOrderRepository.FindAsync(o => o.Status == RentalStatus.PendingPayment &&
                o.CreatedAt <= now - _pendingTimeout);
            var expiredOrders = orders.ToList();
            if (expiredOrders.Count > 0)
            {
                rentalOrderRepository.RemoveRange(expiredOrders);
                await rentalOrderRepository.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} expired pending payment orders.", expiredOrders.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up pending payment orders.");
        }
    }
}
