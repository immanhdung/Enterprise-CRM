using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/alerts")]
public class AlertsController : BaseApiController
{
    private readonly IAlertService _alertService;
    private readonly ICurrentUserService _currentUserService;

    public AlertsController(IAlertService alertService, ICurrentUserService currentUserService)
    {
        _alertService = alertService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BusinessAlertDto>>>> GetActiveAlerts(
        [FromQuery] string? category = null,
        [FromQuery] string? severity = null,
        CancellationToken ct = default)
    {
        var result = await _alertService.GetActiveAlertsAsync(category, severity, ct);
        return OkResponse(result);
    }

    [HttpPost("{id}/acknowledge")]
    public async Task<ActionResult<ApiResponse<BusinessAlertDto>>> AcknowledgeAlert(
        Guid id,
        [FromBody] AcknowledgeAlertRequest? req,
        CancellationToken ct = default)
    {
        var result = await _alertService.AcknowledgeAlertAsync(id, req?.ResolutionNotes, ct);
        if (result == null) return FailResponse<BusinessAlertDto>("Alert not found.", StatusCodes.Status404NotFound);
        return OkResponse(result, "Alert acknowledged.");
    }

    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<ApiResponse<BusinessAlertDto>>> ResolveAlert(
        Guid id,
        [FromBody] AcknowledgeAlertRequest? req,
        CancellationToken ct = default)
    {
        var result = await _alertService.ResolveAlertAsync(id, req?.ResolutionNotes, ct);
        if (result == null) return FailResponse<BusinessAlertDto>("Alert not found.", StatusCodes.Status404NotFound);
        return OkResponse(result, "Alert resolved.");
    }

    [HttpPost("scan")]
    public async Task<ActionResult<ApiResponse<int>>> RunAlertScan(CancellationToken ct = default)
    {
        var count = await _alertService.ScanAndTriggerAlertsAsync(ct);
        return OkResponse(count, $"{count} new business alert(s) generated.");
    }

    [HttpGet("rules")]
    public async Task<ActionResult<ApiResponse<List<AlertRuleDto>>>> GetRules(CancellationToken ct = default)
    {
        var result = await _alertService.GetRulesAsync(ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("rules")]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> CreateRule(
        [FromBody] CreateAlertRuleRequest request,
        CancellationToken ct = default)
    {
        var result = await _alertService.CreateRuleAsync(request, ct);
        return OkResponse(result, "Alert rule created.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPut("rules/{id}")]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> UpdateRule(
        Guid id,
        [FromBody] CreateAlertRuleRequest request,
        CancellationToken ct = default)
    {
        var result = await _alertService.UpdateRuleAsync(id, request, ct);
        return OkResponse(result, "Alert rule updated.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpDelete("rules/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRule(Guid id, CancellationToken ct = default)
    {
        var result = await _alertService.DeleteRuleAsync(id, ct);
        return OkResponse(result, "Alert rule deleted.");
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> GetPreferences(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _alertService.GetUserPreferencesAsync(userId, ct);
        return OkResponse(result);
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> UpdatePreferences(
        [FromBody] UpdateNotificationPreferenceRequest request,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _alertService.UpdateUserPreferencesAsync(userId, request, ct);
        return OkResponse(result, "Notification preferences updated.");
    }
}
