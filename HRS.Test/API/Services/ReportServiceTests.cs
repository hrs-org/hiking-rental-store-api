using FluentAssertions;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using NSubstitute;

namespace HRS.Test.API.Services;

public class ReportServiceTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _service = new ReportService(_paymentRepository);
    }

    [Fact]
    public async Task GenerateReportAsync_WithPayments_ReturnsCorrectReportData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 3);
        var payments = new List<Payment>
        {
            new() { Id = 1, Amount = 100.50m, PaymentDate = new DateTime(2025, 1, 1, 10, 0, 0) },
            new() { Id = 2, Amount = 200.75m, PaymentDate = new DateTime(2025, 1, 1, 14, 0, 0) },
            new() { Id = 3, Amount = 150.25m, PaymentDate = new DateTime(2025, 1, 3, 9, 0, 0) }
        };
        _paymentRepository.GetByDateRangeAsync(startDate, endDate).Returns(payments);

        // Act
        var result = await _service.GenerateReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3); // 3 days from Jan 1 to Jan 3
        result.Data.ElementAt(0).Date.Should().Be(new DateTime(2025, 1, 1));
        result.Data.ElementAt(0).Sales.Should().Be(301.25); // 100.50 + 200.75
        result.Data.ElementAt(1).Date.Should().Be(new DateTime(2025, 1, 2));
        result.Data.ElementAt(1).Sales.Should().Be(0); // No payments
        result.Data.ElementAt(2).Date.Should().Be(new DateTime(2025, 1, 3));
        result.Data.ElementAt(2).Sales.Should().Be(150.25);
        result.TotalSales.Should().Be(451.50); // 301.25 + 0 + 150.25
        result.MaxSales.Should().Be(301.25);
        result.MinSales.Should().Be(0);
    }

    [Fact]
    public async Task GenerateReportAsync_WithNoPayments_ReturnsZeroSalesForAllDates()
    {
        // Arrange
        var startDate = new DateTime(2025, 10, 1);
        var endDate = new DateTime(2025, 10, 3);
        _paymentRepository.GetByDateRangeAsync(startDate, endDate).Returns(new List<Payment>());

        // Act
        var result = await _service.GenerateReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data.Should().AllSatisfy(d => d.Sales.Should().Be(0));
        result.TotalSales.Should().Be(0);
        result.MaxSales.Should().Be(0);
        result.MinSales.Should().Be(0);
    }

    [Fact]
    public async Task GenerateReportAsync_WithSingleDayRange_ReturnsSingleDayReport()
    {
        // Arrange
        var date = new DateTime(2025, 5, 15);
        var payments = new List<Payment>
        {
            new() { Id = 1, Amount = 99.99m, PaymentDate = date }
        };
        _paymentRepository.GetByDateRangeAsync(date, date).Returns(payments);

        // Act
        var result = await _service.GenerateReportAsync(date, date);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Date.Should().Be(date.Date);
        result.Data.First().Sales.Should().Be(99.99);
        result.TotalSales.Should().Be(99.99);
        result.MaxSales.Should().Be(99.99);
        result.MinSales.Should().Be(99.99);
    }

    [Fact]
    public async Task GenerateReportAsync_GroupsPaymentsByDate_IgnoringTime()
    {
        // Arrange
        var startDate = new DateTime(2025, 3, 10);
        var endDate = new DateTime(2025, 3, 10);
        var payments = new List<Payment>
        {
            new() { Id = 1, Amount = 50m, PaymentDate = new DateTime(2025, 3, 10, 8, 30, 0) },
            new() { Id = 2, Amount = 75m, PaymentDate = new DateTime(2025, 3, 10, 15, 45, 0) },
            new() { Id = 3, Amount = 25m, PaymentDate = new DateTime(2025, 3, 10, 23, 59, 59) }
        };
        _paymentRepository.GetByDateRangeAsync(startDate, endDate).Returns(payments);

        // Act
        var result = await _service.GenerateReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Sales.Should().Be(150); // All payments grouped to same date
        result.TotalSales.Should().Be(150);
    }

    [Fact]
    public async Task GenerateReportAsync_CallsRepositoryWithCorrectDateRange()
    {
        // Arrange
        var startDate = new DateTime(2025, 6, 1);
        var endDate = new DateTime(2025, 6, 30);
        _paymentRepository.GetByDateRangeAsync(startDate, endDate).Returns(new List<Payment>());

        // Act
        await _service.GenerateReportAsync(startDate, endDate);

        // Assert
        await _paymentRepository.Received(1).GetByDateRangeAsync(
            Arg.Is<DateTime>(d => d == startDate),
            Arg.Is<DateTime>(d => d == endDate)
        );
    }

    [Fact]
    public async Task GenerateReportAsync_IncludesAllDatesInRange_EvenWithoutPayments()
    {
        // Arrange
        var startDate = new DateTime(2025, 7, 1);
        var endDate = new DateTime(2025, 7, 5);
        var payments = new List<Payment>
        {
            new() { Id = 1, Amount = 100m, PaymentDate = new DateTime(2025, 7, 1) },
            new() { Id = 2, Amount = 200m, PaymentDate = new DateTime(2025, 7, 5) }
            // No payments for July 2, 3, 4
        };
        _paymentRepository.GetByDateRangeAsync(startDate, endDate).Returns(payments);

        // Act
        var result = await _service.GenerateReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(5); // All 5 days included
        result.Data.ElementAt(0).Sales.Should().Be(100);
        result.Data.ElementAt(1).Sales.Should().Be(0);
        result.Data.ElementAt(2).Sales.Should().Be(0);
        result.Data.ElementAt(3).Sales.Should().Be(0);
        result.Data.ElementAt(4).Sales.Should().Be(200);
    }
}
