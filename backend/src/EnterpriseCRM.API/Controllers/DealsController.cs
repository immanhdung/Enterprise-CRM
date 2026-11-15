using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Deals;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/deals")]
[Authorize]
public class DealsController : BaseApiController
{
    private readonly IDealService _dealService;

    public DealsController(IDealService dealService)
    {
        _dealService = dealService;
    }

    [HttpGet]
    [HasPermission(Permissions.DealsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<DealDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeals(
        [FromQuery] string? search,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? stageId,
        [FromQuery] DealStatus? status,
        [FromQuery] Guid? assignedEmployeeId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _dealService.GetDealsAsync(
            search, accountId, stageId, status, assignedEmployeeId, fromDate, toDate,
            sortBy, sortDescending, pageIndex, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("kanban")]
    [HasPermission(Permissions.DealsView)]
    [ProducesResponseType(typeof(ApiResponse<KanbanBoardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKanbanBoard(
        [FromQuery] Guid? assignedEmployeeId,
        [FromQuery] DealStatus? status,
        CancellationToken cancellationToken = default)
    {
        var result = await _dealService.GetKanbanBoardAsync(assignedEmployeeId, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.DealsView)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dealService.GetDealByIdAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.DealsCreate)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealRequest request, CancellationToken cancellationToken)
    {
        var result = await _dealService.CreateDealAsync(request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetDealById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.DealsEdit)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeal(Guid id, [FromBody] UpdateDealRequest request, CancellationToken cancellationToken)
    {
        var result = await _dealService.UpdateDealAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPut("{id:guid}/stage")]
    [HasPermission(Permissions.DealsStageChange)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DealDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDealStage(Guid id, [FromBody] UpdateDealStageRequest request, CancellationToken cancellationToken)
    {
        var result = await _dealService.UpdateDealStageAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.DealsDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeal(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dealService.DeleteDealAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}
