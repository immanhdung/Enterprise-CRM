using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/commissions")]
public class CommissionsController : BaseApiController
{
    private readonly ICommissionService _commissionService;

    public CommissionsController(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    // Policies
    [HttpGet("policies")]
    public async Task<ActionResult<ApiResponse<List<CommissionPolicyDto>>>> GetPolicies(
        [FromQuery] bool? activeOnly,
        CancellationToken ct = default)
    {
        var result = await _commissionService.GetPoliciesAsync(activeOnly, ct);
        return OkResponse(result);
    }

    [HttpGet("policies/{id}")]
    public async Task<ActionResult<ApiResponse<CommissionPolicyDto>>> GetPolicyById(Guid id, CancellationToken ct = default)
    {
        var result = await _commissionService.GetPolicyByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("policies")]
    public async Task<ActionResult<ApiResponse<CommissionPolicyDto>>> CreatePolicy(
        [FromBody] CreateCommissionPolicyRequest req,
        CancellationToken ct = default)
    {
        var result = await _commissionService.CreatePolicyAsync(req, ct);
        return OkResponse(result, "Commission policy created successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPut("policies/{id}")]
    public async Task<ActionResult<ApiResponse<CommissionPolicyDto>>> UpdatePolicy(
        Guid id,
        [FromBody] UpdateCommissionPolicyRequest req,
        CancellationToken ct = default)
    {
        var result = await _commissionService.UpdatePolicyAsync(id, req, ct);
        return OkResponse(result, "Commission policy updated successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpDelete("policies/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePolicy(Guid id, CancellationToken ct = default)
    {
        var result = await _commissionService.DeletePolicyAsync(id, ct);
        return OkResponse(result, "Commission policy deleted successfully.");
    }

    // Commission Calculations & Lifecycle
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CommissionCalculationItemDto>>>> GetCommissions(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _commissionService.GetCommissionsAsync(month, year, employeeId, status, ct);
        return OkResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CommissionCalculationItemDto>>> GetCommissionById(Guid id, CancellationToken ct = default)
    {
        var result = await _commissionService.GetCommissionByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("calculate")]
    public async Task<ActionResult<ApiResponse<List<CommissionCalculationItemDto>>>> CalculateCommissions(
        [FromBody] RunCommissionCalculationRequest req,
        CancellationToken ct = default)
    {
        var result = await _commissionService.CalculateCommissionsAsync(req, ct);
        return OkResponse(result, "Commission calculation completed successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<CommissionCalculationItemDto>>> ApproveCommission(
        Guid id,
        [FromBody] ApproveCommissionRequest req,
        CancellationToken ct = default)
    {
        var result = await _commissionService.ApproveCommissionAsync(id, req, ct);
        return OkResponse(result, "Commission approval decision recorded.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("adjust")]
    public async Task<ActionResult<ApiResponse<CommissionCalculationItemDto>>> AdjustCommission(
        [FromBody] CommissionAdjustmentRequest req,
        CancellationToken ct = default)
    {
        var result = await _commissionService.AdjustCommissionAsync(req, ct);
        return OkResponse(result, "Commission adjusted successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("{id}/recalculate")]
    public async Task<ActionResult<ApiResponse<CommissionCalculationItemDto>>> RecalculateCommission(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await _commissionService.RecalculateCommissionAsync(id, ct);
        return OkResponse(result, "Commission recalculated successfully.");
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<List<CompensationAuditLogDto>>>> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromServices] ICompensationAuditService auditService,
        CancellationToken ct = default)
    {
        var result = await auditService.GetAuditLogsAsync(entityType, entityId, ct);
        return OkResponse(result);
    }
}
