using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Leads;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/leads")]
[Authorize]
public class LeadsController : BaseApiController
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<LeadDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeads(
        [FromQuery] string? search,
        [FromQuery] LeadStatus? status,
        [FromQuery] Guid? sourceId,
        [FromQuery] Guid? assignedEmployeeId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _leadService.GetLeadsAsync(
            search, status, sourceId, assignedEmployeeId, fromDate, toDate,
            sortBy, sortDescending, pageIndex, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeadById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leadService.GetLeadByIdAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost("check-duplicate")]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<DuplicateCheckResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckDuplicate([FromBody] DuplicateCheckRequest request, CancellationToken cancellationToken)
    {
        var result = await _leadService.CheckDuplicateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.LeadsCreate)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var result = await _leadService.CreateLeadAsync(request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetLeadById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLead(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken cancellationToken)
    {
        var result = await _leadService.UpdateLeadAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.LeadsDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leadService.DeleteLeadAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign")]
    [HasPermission(Permissions.LeadsAssign)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignLead(Guid id, [FromBody] AssignLeadRequest request, CancellationToken cancellationToken)
    {
        var result = await _leadService.AssignLeadAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost("{id:guid}/convert")]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<ConvertLeadResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ConvertLeadResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConvertLead(Guid id, [FromBody] ConvertLeadRequest request, CancellationToken cancellationToken)
    {
        var result = await _leadService.ConvertLeadAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpGet("{id:guid}/activities")]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LeadActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivities(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leadService.GetLeadActivitiesAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/activities")]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<LeadActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddActivity(Guid id, [FromBody] CreateLeadActivityRequest request, CancellationToken cancellationToken)
    {
        var result = await _leadService.AddLeadActivityAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpGet("export")]
    [HasPermission(Permissions.LeadsExport)]
    public async Task<IActionResult> ExportLeads(
        [FromQuery] string? search,
        [FromQuery] LeadStatus? status,
        [FromQuery] Guid? sourceId,
        [FromQuery] Guid? assignedEmployeeId,
        CancellationToken cancellationToken)
    {
        var bytes = await _leadService.ExportLeadsCsvAsync(search, status, sourceId, assignedEmployeeId, cancellationToken);
        return File(bytes, "text/csv", $"leads_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost("import")]
    [HasPermission(Permissions.LeadsCreate)]
    [ProducesResponseType(typeof(ApiResponse<LeadImportResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportLeads([FromBody] List<LeadImportItem> items, CancellationToken cancellationToken)
    {
        var result = await _leadService.ImportLeadsAsync(items, cancellationToken);
        return Ok(result);
    }
}
