using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/reports")]
public class ReportsController : BaseApiController
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("scheduled")]
    public async Task<ActionResult<ApiResponse<List<ScheduledReportDto>>>> GetScheduledReports(CancellationToken ct = default)
    {
        var result = await _reportingService.GetScheduledReportsAsync(ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("scheduled")]
    public async Task<ActionResult<ApiResponse<ScheduledReportDto>>> CreateScheduledReport(
        [FromBody] CreateScheduledReportRequest request,
        CancellationToken ct = default)
    {
        var result = await _reportingService.CreateScheduledReportAsync(request, ct);
        return OkResponse(result, "Scheduled report created.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpDelete("scheduled/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteScheduledReport(Guid id, CancellationToken ct = default)
    {
        var success = await _reportingService.DeleteScheduledReportAsync(id, ct);
        return OkResponse(success, "Scheduled report deleted.");
    }

    [HttpGet("generated")]
    public async Task<ActionResult<ApiResponse<List<GeneratedReportDto>>>> GetGeneratedReports(
        [FromQuery] string? reportType = null,
        [FromQuery] int limit = 30,
        CancellationToken ct = default)
    {
        var result = await _reportingService.GetGeneratedReportsAsync(reportType, limit, ct);
        return OkResponse(result);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<GeneratedReportDto>>> GenerateReport(
        [FromBody] GenerateReportRequest request,
        CancellationToken ct = default)
    {
        var result = await _reportingService.GenerateReportAsync(request, ct);
        return OkResponse(result, "Report generated successfully.");
    }

    [HttpGet("data")]
    public async Task<ActionResult<ApiResponse<ManagementReportDataDto>>> GetReportData(
        [FromQuery] string reportType,
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        CancellationToken ct = default)
    {
        var result = await _reportingService.GetReportDataAsync(reportType, periodStart, periodEnd, ct);
        return OkResponse(result);
    }
}
