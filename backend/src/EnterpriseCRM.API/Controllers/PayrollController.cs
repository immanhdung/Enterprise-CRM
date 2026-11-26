using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/payroll")]
public class PayrollController : BaseApiController
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    // Periods
    [HttpGet("periods")]
    public async Task<ActionResult<ApiResponse<List<PayrollPeriodDto>>>> GetPeriods(
        [FromQuery] int? year,
        CancellationToken ct = default)
    {
        var result = await _payrollService.GetPeriodsAsync(year, ct);
        return OkResponse(result);
    }

    [HttpGet("periods/{id}")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> GetPeriodById(Guid id, CancellationToken ct = default)
    {
        var result = await _payrollService.GetPeriodByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("periods")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> CreatePeriod(
        [FromBody] CreatePayrollPeriodRequest req,
        CancellationToken ct = default)
    {
        var result = await _payrollService.CreatePeriodAsync(req, ct);
        return OkResponse(result, "Payroll period created successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("periods/{id}/process")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> ProcessPayroll(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await _payrollService.ProcessPeriodPayrollAsync(id, ct);
        return OkResponse(result, "Payroll processed successfully for period.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant},{SystemRoles.Manager}")]
    [HttpPost("periods/{id}/approve")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> ApprovePayroll(
        Guid id,
        [FromBody] ApprovePayrollRequest req,
        CancellationToken ct = default)
    {
        var result = await _payrollService.ApprovePeriodAsync(id, req, ct);
        return OkResponse(result, "Payroll period approval decision recorded.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpPost("periods/{id}/finalize")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> FinalizePayroll(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await _payrollService.FinalizePeriodAsync(id, ct);
        return OkResponse(result, "Payroll period finalized and disbursements locked.");
    }

    // Records
    [HttpGet("periods/{periodId}/records")]
    public async Task<ActionResult<ApiResponse<List<PayrollRecordDto>>>> GetPayrollRecords(
        Guid periodId,
        [FromQuery] Guid? employeeId,
        CancellationToken ct = default)
    {
        var result = await _payrollService.GetPayrollRecordsAsync(periodId, employeeId, ct);
        return OkResponse(result);
    }

    [HttpGet("records/{id}")]
    public async Task<ActionResult<ApiResponse<PayrollRecordDto>>> GetPayrollRecordById(Guid id, CancellationToken ct = default)
    {
        var result = await _payrollService.GetPayrollRecordByIdAsync(id, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("adjustments")]
    public async Task<ActionResult<ApiResponse<PayrollRecordDto>>> AddAdjustment(
        [FromBody] AddPayrollAdjustmentRequest req,
        CancellationToken ct = default)
    {
        var result = await _payrollService.AddAdjustmentAsync(req, ct);
        return OkResponse(result, "Compensation adjustment added successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Accountant}")]
    [HttpPost("records/{id}/recalculate")]
    public async Task<ActionResult<ApiResponse<PayrollRecordDto>>> RecalculateRecord(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await _payrollService.RecalculateRecordAsync(id, ct);
        return OkResponse(result, "Payroll record recalculated successfully.");
    }
}
