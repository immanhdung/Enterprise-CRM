using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/lead-assignment")]
public class LeadAssignmentController : BaseApiController
{
    private readonly ILeadAssignmentEngine _assignmentEngine;

    public LeadAssignmentController(ILeadAssignmentEngine assignmentEngine)
    {
        _assignmentEngine = assignmentEngine;
    }

    [HttpGet("rules")]
    public async Task<ActionResult<ApiResponse<List<LeadAssignmentRuleDto>>>> GetRules(CancellationToken ct = default)
    {
        var result = await _assignmentEngine.GetRulesAsync(ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("rules")]
    public async Task<ActionResult<ApiResponse<LeadAssignmentRuleDto>>> CreateRule(
        [FromBody] CreateLeadAssignmentRuleRequest request,
        CancellationToken ct = default)
    {
        var result = await _assignmentEngine.CreateRuleAsync(request, ct);
        return OkResponse(result, "Lead assignment rule created.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPut("rules/{id}")]
    public async Task<ActionResult<ApiResponse<LeadAssignmentRuleDto>>> UpdateRule(
        Guid id,
        [FromBody] CreateLeadAssignmentRuleRequest request,
        CancellationToken ct = default)
    {
        var result = await _assignmentEngine.UpdateRuleAsync(id, request, ct);
        return OkResponse(result, "Lead assignment rule updated.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpDelete("rules/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRule(Guid id, CancellationToken ct = default)
    {
        var result = await _assignmentEngine.DeleteRuleAsync(id, ct);
        return OkResponse(result, "Lead assignment rule deleted.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("assign")]
    public async Task<ActionResult<ApiResponse<LeadAssignmentLogDto>>> AssignLead(
        [FromBody] AssignLeadRequest request,
        CancellationToken ct = default)
    {
        var result = await _assignmentEngine.AssignLeadAsync(request, ct);
        return OkResponse(result, "Lead successfully assigned.");
    }

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<List<LeadAssignmentLogDto>>>> GetLogs(
        [FromQuery] Guid? leadId = null,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var result = await _assignmentEngine.GetAssignmentLogsAsync(leadId, limit, ct);
        return OkResponse(result);
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<ApiResponse<LeadDistributionMetricsDto>>> GetMetrics(CancellationToken ct = default)
    {
        var result = await _assignmentEngine.GetDistributionMetricsAsync(ct);
        return OkResponse(result);
    }
}
