using HRS.API.Contracts.DTOs.Report;

namespace HRS.API.Services.Interfaces;

public interface IReportService
{
    Task<ReportResponseDto> GenerateReportAsync(DateTime startDate, DateTime endDate);
}
