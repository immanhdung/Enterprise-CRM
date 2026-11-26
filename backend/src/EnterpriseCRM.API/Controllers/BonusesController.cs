using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/bonuses")]
public class BonusesController : BaseApiController
{
    private readonly IBonusService _bonusService;

    public BonusesController(IBonusService bonusService)
    {
        _bonusService = bonusService;
    }

    // Policies
    [HttpGet("policies")]
    public async Task<ActionResult<ApiResponse<List<BonusPolicyDto>>>> GetPolicies(
        [FromQuery] bool? activeOnly,
        CancellationToken ct = default)
    {
        var result = await _bonusService.GetPoliciesAsync(activeOnly, ct);
        return OkResponse(result);
    }

    [HttpGet("policies/{id}")]
    public async Task<ActionResult<ApiResponse<BonusPolicyDto>>> GetPolicyById(Guid id, CancellationToken ct = default)
    {
        var result = await _bonusService.GetPolicyByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("policies")]
    public async Task<ActionResult<ApiResponse<BonusPolicyDto>>> CreatePolicy(
        [FromBody] CreateBonusPolicyRequest req,
        CancellationToken ct = default)
    {
        var result = await _bonusService.CreatePolicyAsync(req, ct);
        return OkResponse(result, "Bonus policy created successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPut("policies/{id}")]
    public async Task<ActionResult<ApiResponse<BonusPolicyDto>>> UpdatePolicy(
        Guid id,
        [FromBody] UpdateBonusPolicyRequest req,
        CancellationToken ct = default)
    {
        var result = await _bonusService.UpdatePolicyAsync(id, req, ct);
        return OkResponse(result, "Bonus policy updated successfully.");
    }

    // Bonus Records
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BonusRecordDto>>>> GetBonusRecords(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _bonusService.GetBonusRecordsAsync(month, year, employeeId, status, ct);
        return OkResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BonusRecordDto>>> GetBonusRecordById(Guid id, CancellationToken ct = default)
    {
        var result = await _bonusService.GetBonusRecordByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant},{SystemRoles.Manager}")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BonusRecordDto>>> CreateBonusRecord(
        [FromBody] CreateBonusRecordRequest req,
        CancellationToken ct = default)
    {
        var result = await _bonusService.CreateBonusRecordAsync(req, ct);
        return OkResponse(result, "Bonus record created successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<BonusRecordDto>>> ApproveBonusRecord(
        Guid id,
        [FromBody] ApproveBonusRecordRequest req,
        CancellationToken ct = default)
    {
        var result = await _bonusService.ApproveBonusRecordAsync(id, req, ct);
        return OkResponse(result, "Bonus approval decision recorded.");
    }
}
