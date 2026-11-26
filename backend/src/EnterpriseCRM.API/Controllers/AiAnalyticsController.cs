using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/ai-analytics")]
public class AiAnalyticsController : BaseApiController
{
    private readonly ISalesAnalyticsService _salesAnalyticsService;
    private readonly IRevenueForecastingService _forecastingService;
    private readonly IAnomalyDetectionService _anomalyService;

    public AiAnalyticsController(
        ISalesAnalyticsService salesAnalyticsService,
        IRevenueForecastingService forecastingService,
        IAnomalyDetectionService anomalyService)
    {
        _salesAnalyticsService = salesAnalyticsService;
        _forecastingService = forecastingService;
        _anomalyService = anomalyService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<SalesAnalysisSummaryDto>>> GetOverview(
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        CancellationToken ct = default)
    {
        var result = await _salesAnalyticsService.GetPerformanceAnalysisAsync(month, year, ct);
        return OkResponse(result);
    }

    [HttpGet("forecast")]
    public async Task<ActionResult<ApiResponse<RevenueForecastDto>>> GetForecast(
        [FromQuery] int horizonDays = 30,
        CancellationToken ct = default)
    {
        var result = await _forecastingService.GetForecastAsync(horizonDays, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("forecast/recalculate")]
    public async Task<ActionResult<ApiResponse<RevenueForecastDto>>> RecalculateForecast(
        [FromBody] GenerateForecastRequest req,
        CancellationToken ct = default)
    {
        var result = await _forecastingService.RecalculateForecastAsync(req.HorizonDays, ct);
        return OkResponse(result, "Revenue forecast recalculated.");
    }

    [HttpGet("anomalies")]
    public async Task<ActionResult<ApiResponse<List<AnomalyDetectionDto>>>> GetAnomalies(
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _anomalyService.GetAnomaliesAsync(category, status, ct);
        return OkResponse(result);
    }

    [HttpPost("anomalies/scan")]
    public async Task<ActionResult<ApiResponse<int>>> RunAnomalyScan(CancellationToken ct = default)
    {
        var count = await _anomalyService.RunAnomalyScanAsync(ct);
        return OkResponse(count, $"{count} anomaly item(s) scanned and updated.");
    }

    [HttpPost("anomalies/{id}/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveAnomaly(
        Guid id,
        [FromBody] ResolveAnomalyRequest request,
        CancellationToken ct = default)
    {
        var success = await _anomalyService.ResolveAnomalyAsync(id, request, ct);
        return OkResponse(success, "Anomaly updated.");
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<ApiResponse<List<BusinessRecommendationDto>>>> GetRecommendations(
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _salesAnalyticsService.GetRecommendationsAsync(status, ct);
        return OkResponse(result);
    }

    [HttpPost("recommendations/{id}/apply")]
    public async Task<ActionResult<ApiResponse<bool>>> ApplyRecommendation(
        Guid id,
        [FromBody] ApplyRecommendationRequest request,
        CancellationToken ct = default)
    {
        var success = await _salesAnalyticsService.ApplyRecommendationAsync(id, request, ct);
        return OkResponse(success, $"Recommendation marked as {request.Action}.");
    }
}
