namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public interface IPayrollService
{
    // Periods
    Task<List<PayrollPeriodDto>> GetPeriodsAsync(int? year = null, CancellationToken ct = default);
    Task<PayrollPeriodDto> GetPeriodByIdAsync(Guid id, CancellationToken ct = default);
    Task<PayrollPeriodDto> CreatePeriodAsync(CreatePayrollPeriodRequest req, CancellationToken ct = default);
    Task<PayrollPeriodDto> ProcessPeriodPayrollAsync(Guid periodId, CancellationToken ct = default);
    Task<PayrollPeriodDto> ApprovePeriodAsync(Guid periodId, ApprovePayrollRequest req, CancellationToken ct = default);
    Task<PayrollPeriodDto> FinalizePeriodAsync(Guid periodId, CancellationToken ct = default);

    // Records
    Task<List<PayrollRecordDto>> GetPayrollRecordsAsync(Guid periodId, Guid? employeeId = null, CancellationToken ct = default);
    Task<PayrollRecordDto> GetPayrollRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task<PayrollRecordDto> AddAdjustmentAsync(AddPayrollAdjustmentRequest req, CancellationToken ct = default);
    Task<PayrollRecordDto> RecalculateRecordAsync(Guid recordId, CancellationToken ct = default);
}
