namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public interface ICommissionService
{
    // Policies
    Task<List<CommissionPolicyDto>> GetPoliciesAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<CommissionPolicyDto> GetPolicyByIdAsync(Guid id, CancellationToken ct = default);
    Task<CommissionPolicyDto> CreatePolicyAsync(CreateCommissionPolicyRequest req, CancellationToken ct = default);
    Task<CommissionPolicyDto> UpdatePolicyAsync(Guid id, UpdateCommissionPolicyRequest req, CancellationToken ct = default);
    Task<bool> DeletePolicyAsync(Guid id, CancellationToken ct = default);

    // Calculation Engine & Lifecycle
    Task<List<CommissionCalculationItemDto>> CalculateCommissionsAsync(RunCommissionCalculationRequest req, CancellationToken ct = default);
    Task<List<CommissionCalculationItemDto>> GetCommissionsAsync(int? month, int? year, Guid? employeeId, string? status, CancellationToken ct = default);
    Task<CommissionCalculationItemDto> GetCommissionByIdAsync(Guid id, CancellationToken ct = default);
    Task<CommissionCalculationItemDto> ApproveCommissionAsync(Guid id, ApproveCommissionRequest req, CancellationToken ct = default);
    Task<CommissionCalculationItemDto> AdjustCommissionAsync(CommissionAdjustmentRequest req, CancellationToken ct = default);
    Task<CommissionCalculationItemDto> RecalculateCommissionAsync(Guid id, CancellationToken ct = default);
}
