using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/kpis")]
public class KpisController : BaseApiController
{
    private readonly IKpiService _kpiService;

    public KpisController(IKpiService kpiService)
    {
        _kpiService = kpiService;
    }

    // Definitions
    [HttpGet("definitions")]
    public async Task<ActionResult<ApiResponse<List<KpiDefinitionDto>>>> GetDefinitions(CancellationToken ct = default)
    {
        var result = await _kpiService.GetDefinitionsAsync(ct);
        return OkResponse(result);
    }

    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<ApiResponse<KpiDefinitionDto>>> GetDefinitionById(Guid id, CancellationToken ct = default)
    {
        var result = await _kpiService.GetDefinitionByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("definitions")]
    public async Task<ActionResult<ApiResponse<KpiDefinitionDto>>> CreateDefinition(
        [FromBody] CreateKpiDefinitionRequest req,
        CancellationToken ct = default)
    {
        var result = await _kpiService.CreateDefinitionAsync(req, ct);
        return OkResponse(result, "KPI definition created successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPut("definitions/{id}")]
    public async Task<ActionResult<ApiResponse<KpiDefinitionDto>>> UpdateDefinition(
        Guid id,
        [FromBody] UpdateKpiDefinitionRequest req,
        CancellationToken ct = default)
    {
        var result = await _kpiService.UpdateDefinitionAsync(id, req, ct);
        return OkResponse(result, "KPI definition updated successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpDelete("definitions/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDefinition(Guid id, CancellationToken ct = default)
    {
        var result = await _kpiService.DeleteDefinitionAsync(id, ct);
        return OkResponse(result, "KPI definition deleted successfully.");
    }

    // Assignments
    [HttpGet("assignments")]
    public async Task<ActionResult<ApiResponse<List<KpiAssignmentDto>>>> GetAssignments(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _kpiService.GetAssignmentsAsync(month, year, employeeId, departmentId, status, ct);
        return OkResponse(result);
    }

    [HttpGet("assignments/{id}")]
    public async Task<ActionResult<ApiResponse<KpiAssignmentDto>>> GetAssignmentById(Guid id, CancellationToken ct = default)
    {
        var result = await _kpiService.GetAssignmentByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("assignments")]
    public async Task<ActionResult<ApiResponse<KpiAssignmentDto>>> CreateAssignment(
        [FromBody] CreateKpiAssignmentRequest req,
        CancellationToken ct = default)
    {
        var result = await _kpiService.CreateAssignmentAsync(req, ct);
        return OkResponse(result, "KPI target assigned successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPut("assignments/{id}")]
    public async Task<ActionResult<ApiResponse<KpiAssignmentDto>>> UpdateAssignment(
        Guid id,
        [FromBody] UpdateKpiAssignmentRequest req,
        CancellationToken ct = default)
    {
        var result = await _kpiService.UpdateAssignmentAsync(id, req, ct);
        return OkResponse(result, "KPI target updated successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("assignments/{id}/evaluate")]
    public async Task<ActionResult<ApiResponse<KpiAssignmentDto>>> EvaluateAssignment(
        Guid id,
        [FromBody] EvaluateKpiRequest req,
        CancellationToken ct = default)
    {
        var result = await _kpiService.EvaluateAssignmentAsync(id, req, ct);
        return OkResponse(result, "KPI performance evaluated successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("assignments/{id}/approve")]
    public async Task<ActionResult<ApiResponse<KpiAssignmentDto>>> ApproveAssignment(
        Guid id,
        [FromBody] ApproveKpiRequest req,
        CancellationToken ct = default)
    {
        var result = await _kpiService.ApproveAssignmentAsync(id, req, ct);
        return OkResponse(result, "KPI evaluation decision recorded.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost("assignments/{id}/auto-calculate")]
    public async Task<ActionResult<ApiResponse<KpiAssignmentDto>>> AutoCalculateActuals(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await _kpiService.AutoCalculateActualsAsync(id, ct);
        return OkResponse(result, "Actual KPI metrics calculated from database.");
    }
}
