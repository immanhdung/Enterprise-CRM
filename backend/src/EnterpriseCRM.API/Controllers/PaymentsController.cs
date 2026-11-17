using System.Security.Claims;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;

    public PaymentsController(IPaymentService paymentService, ICurrentUserService currentUserService)
    {
        _paymentService = paymentService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedPaymentResult>>> GetPayments(
        [FromQuery] PaymentFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetPaymentsAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);
        if (result == null)
            return FailResponse<PaymentDto>("Payment not found.", StatusCodes.Status404NotFound);

        return OkResponse(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _paymentService.CreatePaymentAsync(request, userId, userName, cancellationToken);
        return OkResponse(result, "Payment recorded successfully in Pending status.");
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ConfirmPayment(
        Guid id,
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _paymentService.ConfirmPaymentAsync(id, request, userId, userName, cancellationToken);
        return OkResponse(result, "Payment confirmed and counted toward collected cash revenue.");
    }

    [HttpPost("{id:guid}/reverse")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ReversePayment(
        Guid id,
        [FromBody] ReversePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";

        var result = await _paymentService.ReversePaymentAsync(id, request, userId, userName, cancellationToken);
        return OkResponse(result, "Payment reversed successfully with financial audit log.");
    }

    [HttpGet("contract/{contractId:guid}/summary")]
    public async Task<ActionResult<ApiResponse<ContractFinancialSummaryDto>>> GetContractFinancialSummary(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetContractFinancialSummaryAsync(contractId, cancellationToken);
        return OkResponse(result);
    }
}
