using HRS.API.Contracts.DTOs.Report;
using HRS.API.Services.Interfaces;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class ReportService : IReportService
{
    private readonly IPaymentRepository _paymentRepository;

    public ReportService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<ReportResponseDto> GenerateReportAsync(DateTime startDate, DateTime endDate)
    {
        var payments = await _paymentRepository.GetByDateRangeAsync(startDate, endDate);

        var paymentsByDate = payments
            .GroupBy(p => p.PaymentDate.Date)
            .ToDictionary(g => g.Key, g => (double)g.Sum(p => p.Amount));

        var allDates = new List<ReportData>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            allDates.Add(new ReportData
            {
                Date = date,
                Sales = paymentsByDate.TryGetValue(date, out var sales) ? sales : 0
            });

        var report = new ReportResponseDto
        {
            Data = allDates.ToArray(),
            MaxSales = allDates.Max(d => d.Sales),
            MinSales = allDates.Min(d => d.Sales),
            TotalSales = allDates.Sum(d => d.Sales)
        };

        return report;
    }
}
