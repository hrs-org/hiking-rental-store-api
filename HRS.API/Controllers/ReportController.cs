using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Report;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<ActionResult<ReportResponseDto>> GetRentalSummaryReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GenerateReportAsync(startDate, endDate);
        return Ok(ApiResponse<ReportResponseDto>.OkResponse(result));
    }
}
