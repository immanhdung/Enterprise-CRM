using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/payment-schedules")]
public class PaymentSchedulesController : BaseApiController
{
    private readonly IPaymentScheduleService _scheduleService;

    public PaymentSchedulesController(IPaymentScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("contract/{contractId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentScheduleDto>>>> GetContractSchedules(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var schedules = await _scheduleService.GetContractSchedulesAsync(contractId, cancellationToken);
        return OkResponse(schedules);
    }

    [HttpPost("contract/{contractId:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentScheduleDto>>> CreateSchedule(
        Guid contractId,
        [FromBody] CreatePaymentScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _scheduleService.CreateScheduleAsync(contractId, request, cancellationToken);
        return OkResponse(result, "Payment schedule installment created successfully.");
    }

    [HttpPost("contract/{contractId:guid}/generate")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentScheduleDto>>>> GenerateContractSchedules(
        Guid contractId,
        [FromBody] GenerateInstallmentSchedulesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _scheduleService.GenerateContractSchedulesAsync(contractId, request, cancellationToken);
        return OkResponse(result, "Contract payment installment plan generated successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSchedule(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _scheduleService.DeleteScheduleAsync(id, cancellationToken);
        if (!deleted)
            return FailResponse<bool>("Payment schedule installment not found or cannot be deleted.", StatusCodes.Status404NotFound);

        return OkResponse(true, "Payment schedule installment deleted successfully.");
    }
}
