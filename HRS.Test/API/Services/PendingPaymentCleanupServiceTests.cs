using System;
using System.Threading.Tasks;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HRS.Test.API.Services;

public class PendingPaymentCleanupServiceTests
{
    [Fact]
    public async Task CleanupOrder_RemovesPendingPaymentOrder()
    {
        // Arrange
        var order = new RentalOrder
        {
            Id = 123,
            Status = RentalStatus.PendingPayment
        };

        var rentalOrderRepo = Substitute.For<IRentalOrderRepository>();
        rentalOrderRepo.GetByIdAsync(order.Id).Returns(order);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(IRentalOrderRepository)).Returns(rentalOrderRepo);
        serviceProvider.CreateScope().Returns(scope);

        var service = new PendingPaymentCleanupService(serviceProvider);

        // Act
        await service.CleanupOrder(order.Id);

        // Assert
        rentalOrderRepo.Received().Remove(order);
        await rentalOrderRepo.Received().SaveChangesAsync();
    }
}
