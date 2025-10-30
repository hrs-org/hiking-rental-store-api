using System;
using System.Threading.Tasks;
using Hangfire;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using HRS.API.Services.Interfaces;

namespace HRS.API.Services;

public interface IPendingPaymentCleanupService
{
    Task RegisterPendingPaymentCleanupAsync(int orderId);
}

public class PendingPaymentCleanupService : IPendingPaymentCleanupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _pendingTimeoutMinutes;

    public PendingPaymentCleanupService(
        IServiceProvider serviceProvider,
        IAppConfiguration appConfig)
    {
        _serviceProvider = serviceProvider;
        _pendingTimeoutMinutes = appConfig.PendingPaymentCleanupTimeoutMinutes;
    }

    public Task RegisterPendingPaymentCleanupAsync(int orderId)
    {
        BackgroundJob.Schedule(() => CleanupOrder(orderId), TimeSpan.FromMinutes(_pendingTimeoutMinutes));
        return Task.CompletedTask;
    }

    public async Task CleanupOrder(int orderId)
    {
        using var scope = _serviceProvider.CreateScope();
        var rentalOrderRepository = scope.ServiceProvider.GetRequiredService<IRentalOrderRepository>();
        var order = await rentalOrderRepository.GetByIdAsync(orderId);
        if (order != null && order.Status == RentalStatus.PendingPayment)
        {
            rentalOrderRepository.Remove(order);
            await rentalOrderRepository.SaveChangesAsync();
        }
    }
}
