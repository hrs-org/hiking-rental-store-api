using System;
using System.Threading.Tasks;
using Hangfire;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HRS.API.Services;

public interface IPendingPaymentCleanupService
{
    Task RegisterPendingPaymentCleanupAsync(int orderId);
}

public class PendingPaymentCleanupService : IPendingPaymentCleanupService
{
    private const int PendingTimeoutMinutes = 1;
    private readonly IServiceProvider _serviceProvider;

    public PendingPaymentCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task RegisterPendingPaymentCleanupAsync(int orderId)
    {
        BackgroundJob.Schedule(() => CleanupOrder(orderId), TimeSpan.FromMinutes(PendingTimeoutMinutes));
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
