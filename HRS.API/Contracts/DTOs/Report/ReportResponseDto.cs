namespace HRS.API.Contracts.DTOs.Report;

public class ReportResponseDto
{
    public required ICollection<ReportData> Data { get; set; }
    public required double MaxSales { get; set; }
    public required double MinSales { get; set; }
    public required double TotalSales { get; set; }
}

public class ReportData
{
    public DateTime Date { get; set; }
    public double Sales { get; set; }
}
