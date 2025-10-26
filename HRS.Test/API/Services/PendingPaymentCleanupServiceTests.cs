using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HRS.Test.API.Services;

public class PendingPaymentCleanupServiceTests
{
    [Fact]
    public async Task ExecuteAsync_RemovesExpiredPendingPaymentOrders()
    {
        // Arrange
        var expiredOrder = new RentalOrder
        {
            Status = RentalStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
        };
        var orders = new List<RentalOrder> { expiredOrder };

        var rentalOrderRepo = Substitute.For<IRentalOrderRepository>();
        rentalOrderRepo.FindAsync(Arg.Any<Expression<Func<RentalOrder, bool>>>())
            .Returns(callInfo => orders.AsQueryable().Where(callInfo.Arg<Expression<Func<RentalOrder, bool>>>().Compile()));

        var serviceProvider = Substitute.For<IServiceProvider>();
        var scope = Substitute.For<IServiceScope>();
        var logger = Substitute.For<ILogger<PendingPaymentCleanupService>>();

        rentalOrderRepo.When(x => x.RemoveRange(Arg.Any<IEnumerable<RentalOrder>>())).Do(_ => { });
        rentalOrderRepo.SaveChangesAsync().Returns(0);

        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(IRentalOrderRepository)).Returns(rentalOrderRepo);

        // Mock IServiceScopeFactory
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);
        serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
        serviceProvider.CreateScope().Returns(scope); // for compatibility if used

        var service = new PendingPaymentCleanupService(serviceProvider, logger);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1200); // 1.2 seconds, enough for one loop

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        rentalOrderRepo.Received().RemoveRange(Arg.Is<IEnumerable<RentalOrder>>(x => x.Contains(expiredOrder)));
        await rentalOrderRepo.Received().SaveChangesAsync();
    }
}
