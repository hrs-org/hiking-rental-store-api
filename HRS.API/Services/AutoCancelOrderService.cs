using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRS.API.Services;
public class AutoCancelOrderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoCancelOrderService> _logger;

    public AutoCancelOrderService(IServiceProvider serviceProvider, ILogger<AutoCancelOrderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoCancelOrderService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var orderRepository = scope.ServiceProvider.GetRequiredService<IRentalOrderRepository>();

                var now = DateTime.UtcNow;
                var expiredOrders = await orderRepository.GetPendingPaymentOrdersOlderThanAsync(now.AddMinutes(-30));

                foreach (var order in expiredOrders)
                {
                    order.Status = RentalStatus.Cancelled;
                    order.ReturnRemarks = "Auto-cancel after 30 minutes of inactivity";
                    order.UpdatedAt = now;
                    order.UpdatedById = 1; // System user ID
                    orderRepository.Update(order);
                    await orderRepository.SaveChangesAsync();
                    // _logger.LogInformation($"Order {order.Id} cancelled automatically.");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
