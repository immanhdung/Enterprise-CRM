using System.Security.Claims;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
public class InvoicesController : BaseApiController
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICurrentUserService _currentUserService;

    public InvoicesController(IInvoiceService invoiceService, ICurrentUserService currentUserService)
    {
        _invoiceService = invoiceService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedInvoiceResult>>> GetInvoices(
        [FromQuery] InvoiceFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.GetInvoicesAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoiceById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.GetInvoiceByIdAsync(id, cancellationToken);
        if (result == null)
            return FailResponse<InvoiceDto>("Invoice not found.", StatusCodes.Status404NotFound);

        return OkResponse(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> IssueInvoice(
        [FromBody] IssueInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _invoiceService.IssueInvoiceAsync(request, userId, userName, cancellationToken);
        return OkResponse(result, "Electronic invoice issued successfully.");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CancelInvoice(
        Guid id,
        [FromBody] CancelInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _invoiceService.CancelInvoiceAsync(id, request, userId, userName, cancellationToken);
        return OkResponse(result, "Invoice cancelled successfully.");
    }

    [HttpPost("{id:guid}/sync")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> SyncInvoiceStatus(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.SyncInvoiceStatusAsync(id, cancellationToken);
        return OkResponse(result, "Invoice status synchronized with electronic invoice provider.");
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _invoiceService.GenerateInvoicePdfAsync(id, cancellationToken);
        return File(bytes, "text/html", $"invoice-{id}.html");
    }

    // Invoice Requests
    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<PagedInvoiceRequestResult>>> GetInvoiceRequests(
        [FromQuery] InvoiceRequestFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.GetInvoiceRequestsAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponse<InvoiceRequestDto>>> CreateInvoiceRequest(
        [FromBody] CreateInvoiceRequestInput request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _invoiceService.CreateInvoiceRequestAsync(request, userId, userName, cancellationToken);
        return OkResponse(result, "Invoice request submitted for accountant review.");
    }

    [HttpPost("requests/{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> ApproveInvoiceRequest(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _invoiceService.ApproveAndIssueInvoiceRequestAsync(id, userId, userName, cancellationToken);
        return OkResponse(result, "Invoice request approved and electronic invoice issued.");
    }

    [HttpPost("requests/{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectInvoiceRequest(
        Guid id,
        [FromBody] string reason,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _invoiceService.RejectInvoiceRequestAsync(id, reason, userId, userName, cancellationToken);
        if (!result)
            return FailResponse<bool>("Invoice request not found.", StatusCodes.Status404NotFound);

        return OkResponse(true, "Invoice request rejected.");
    }
}
