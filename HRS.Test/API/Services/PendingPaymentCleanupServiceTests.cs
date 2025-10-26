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
using Moq;
using Xunit;

namespace HRS.Test.API.Services;

public class PendingPaymentCleanupServiceTests
{
    [Fact]
    public async Task ExecuteAsync_RemovesExpiredPendingPaymentOrders()
    {
        var expiredOrder = new RentalOrder
        {
            Status = RentalStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
        };

        var orders = new List<RentalOrder> { expiredOrder };

        var rentalOrderRepoMock = new Mock<IRentalOrderRepository>();
        rentalOrderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RentalOrder, bool>>>()))
            .ReturnsAsync((Expression<Func<RentalOrder, bool>> predicate) => orders.AsQueryable().Where(predicate.Compile()));
        rentalOrderRepoMock.Setup(r => r.RemoveRange(It.IsAny<IEnumerable<RentalOrder>>()))
            .Verifiable();
        rentalOrderRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0)
            .Verifiable();

        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var loggerMock = new Mock<ILogger<PendingPaymentCleanupService>>();

        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRentalOrderRepository))).Returns(rentalOrderRepoMock.Object);
        serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(scopeMock.Object);

        var service = new PendingPaymentCleanupService(serviceProviderMock.Object, loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1200);

        await service.StartAsync(cts.Token);

        rentalOrderRepoMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<RentalOrder>>()), Times.AtLeastOnce);
        rentalOrderRepoMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
    }
}
