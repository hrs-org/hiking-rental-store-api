using FluentAssertions;
using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Report;
using HRS.API.Controllers;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace HRS.Test.API.Controllers;

public class ReportControllerTests
{
    private readonly ReportController _controller;
    private readonly IReportService _service;

    public ReportControllerTests()
    {
        _service = Substitute.For<IReportService>();
        _controller = new ReportController(_service);
    }

    [Fact]
    public async Task GetRentalSummaryReport_ReturnsOkWithReportData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var reportResponse = new ReportResponseDto
        {
            Data = new List<ReportData>
            {
                new() { Date = new DateTime(2025, 1, 1), Sales = 100.0 },
                new() { Date = new DateTime(2025, 1, 2), Sales = 200.0 }
            },
            MaxSales = 200.0,
            MinSales = 100.0,
            TotalSales = 300.0
        };
        _service.GenerateReportAsync(startDate, endDate).Returns(reportResponse);

        // Act
        var result = await _controller.GetRentalSummaryReport(startDate, endDate);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var apiResponse = okResult!.Value as ApiResponse<ReportResponseDto>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().BeEquivalentTo(reportResponse);
        apiResponse.Success.Should().BeTrue();
        await _service.Received(1).GenerateReportAsync(startDate, endDate);
    }

    [Fact]
    public async Task GetRentalSummaryReport_CallsServiceWithCorrectDates()
    {
        // Arrange
        var startDate = new DateTime(2025, 10, 1);
        var endDate = new DateTime(2025, 10, 29);
        var reportResponse = new ReportResponseDto
        {
            Data = new List<ReportData>(),
            MaxSales = 0,
            MinSales = 0,
            TotalSales = 0
        };
        _service.GenerateReportAsync(startDate, endDate).Returns(reportResponse);

        // Act
        await _controller.GetRentalSummaryReport(startDate, endDate);

        // Assert
        await _service.Received(1).GenerateReportAsync(
            Arg.Is<DateTime>(d => d == startDate),
            Arg.Is<DateTime>(d => d == endDate)
        );
    }
}
