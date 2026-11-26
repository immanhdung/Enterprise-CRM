using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/performance")]
public class PerformanceController : BaseApiController
{
    private readonly ISalesPerformanceService _performanceService;

    public PerformanceController(ISalesPerformanceService performanceService)
    {
        _performanceService = performanceService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<SalesPerformanceSummaryDto>>> GetPerformanceSummary(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? teamId,
        CancellationToken ct = default)
    {
        var result = await _performanceService.GetPerformanceSummaryAsync(month, year, employeeId, departmentId, teamId, ct);
        return OkResponse(result);
    }

    [HttpGet("rankings")]
    public async Task<ActionResult<ApiResponse<List<PerformanceRankingItemDto>>>> GetRankings(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? departmentId,
        CancellationToken ct = default)
    {
        var result = await _performanceService.GetEmployeeRankingsAsync(month, year, departmentId, ct);
        return OkResponse(result);
    }

    [HttpGet("timeline")]
    public async Task<ActionResult<ApiResponse<List<PerformanceTimelineItemDto>>>> GetTimeline(
        [FromQuery] string? timeframe,
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId,
        CancellationToken ct = default)
    {
        var tf = string.IsNullOrWhiteSpace(timeframe) ? "monthly" : timeframe;
        var result = await _performanceService.GetTimelineAnalyticsAsync(tf, year, employeeId, departmentId, ct);
        return OkResponse(result);
    }
}
