namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public interface IBonusService
{
    // Policies
    Task<List<BonusPolicyDto>> GetPoliciesAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<BonusPolicyDto> GetPolicyByIdAsync(Guid id, CancellationToken ct = default);
    Task<BonusPolicyDto> CreatePolicyAsync(CreateBonusPolicyRequest req, CancellationToken ct = default);
    Task<BonusPolicyDto> UpdatePolicyAsync(Guid id, UpdateBonusPolicyRequest req, CancellationToken ct = default);

    // Records
    Task<List<BonusRecordDto>> GetBonusRecordsAsync(int? month, int? year, Guid? employeeId, string? status, CancellationToken ct = default);
    Task<BonusRecordDto> GetBonusRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task<BonusRecordDto> CreateBonusRecordAsync(CreateBonusRecordRequest req, CancellationToken ct = default);
    Task<BonusRecordDto> ApproveBonusRecordAsync(Guid id, ApproveBonusRecordRequest req, CancellationToken ct = default);
}
