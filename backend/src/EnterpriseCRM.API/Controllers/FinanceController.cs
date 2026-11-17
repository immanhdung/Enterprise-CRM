using System.Security.Claims;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/finance")]
public class FinanceController : BaseApiController
{
    private readonly IRevenueService _revenueService;
    private readonly IReceivableService _receivableService;
    private readonly IFinancialReconciliationService _reconciliationService;
    private readonly ICurrentUserService _currentUserService;

    public FinanceController(
        IRevenueService revenueService,
        IReceivableService receivableService,
        IFinancialReconciliationService reconciliationService,
        ICurrentUserService currentUserService)
    {
        _revenueService = revenueService;
        _receivableService = receivableService;
        _reconciliationService = reconciliationService;
        _currentUserService = currentUserService;
    }

    // Revenue
    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueAnalyticsResponse>>> GetRevenueAnalytics(
        [FromQuery] RevenueFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _revenueService.GetRevenueAnalyticsAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    // Receivables & Debt Aging
    [HttpGet("receivables/summary")]
    public async Task<ActionResult<ApiResponse<ReceivableAgingSummaryDto>>> GetReceivablesSummary(
        CancellationToken cancellationToken = default)
    {
        var result = await _receivableService.GetAgingSummaryAsync(cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("receivables")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AccountsReceivableItemDto>>>> GetReceivablesList(
        [FromQuery] ReceivableFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _receivableService.GetReceivablesListAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("collection-logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ReceivableCollectionLogDto>>>> GetCollectionLogs(
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? contractId,
        CancellationToken cancellationToken = default)
    {
        var result = await _receivableService.GetCollectionLogsAsync(accountId, contractId, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("collection-logs")]
    public async Task<ActionResult<ApiResponse<ReceivableCollectionLogDto>>> AddCollectionLog(
        [FromBody] CreateReceivableCollectionLogRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _receivableService.AddCollectionLogAsync(request, userId, userName, cancellationToken);
        return OkResponse(result, "Receivable collection activity logged successfully.");
    }

    // Financial Reconciliation
    [HttpGet("reconciliation")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinancialReconciliationRecordDto>>>> GetReconciliationHistory(
        CancellationToken cancellationToken = default)
    {
        var result = await _reconciliationService.GetReconciliationHistoryAsync(cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("reconciliation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<FinancialReconciliationRecordDto>>> GetReconciliationById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _reconciliationService.GetReconciliationByIdAsync(id, cancellationToken);
        if (result == null)
            return FailResponse<FinancialReconciliationRecordDto>("Reconciliation record not found.", StatusCodes.Status404NotFound);

        return OkResponse(result);
    }

    [HttpPost("reconciliation/run")]
    public async Task<ActionResult<ApiResponse<FinancialReconciliationRecordDto>>> RunReconciliation(
        [FromBody] RunReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _reconciliationService.RunReconciliationAsync(request, userId, userName, cancellationToken);
        return OkResponse(result, "Financial reconciliation executed successfully.");
    }

    [HttpPost("reconciliation/items/{itemId:guid}/resolve")]
    public async Task<ActionResult<ApiResponse<ReconciliationItemDto>>> ResolveDiscrepancy(
        Guid itemId,
        [FromBody] ResolveDiscrepancyRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _reconciliationService.ResolveDiscrepancyAsync(itemId, request, userId, userName, cancellationToken);
        return OkResponse(result, "Discrepancy resolution recorded with authorized confirmation.");
    }
}
