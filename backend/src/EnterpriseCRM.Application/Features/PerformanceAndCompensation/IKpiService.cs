namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public interface IKpiService
{
    // KPI Definitions
    Task<List<KpiDefinitionDto>> GetDefinitionsAsync(CancellationToken ct = default);
    Task<KpiDefinitionDto> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default);
    Task<KpiDefinitionDto> CreateDefinitionAsync(CreateKpiDefinitionRequest req, CancellationToken ct = default);
    Task<KpiDefinitionDto> UpdateDefinitionAsync(Guid id, UpdateKpiDefinitionRequest req, CancellationToken ct = default);
    Task<bool> DeleteDefinitionAsync(Guid id, CancellationToken ct = default);

    // KPI Assignments & Evaluations
    Task<List<KpiAssignmentDto>> GetAssignmentsAsync(int? month, int? year, Guid? employeeId, Guid? departmentId, string? status, CancellationToken ct = default);
    Task<KpiAssignmentDto> GetAssignmentByIdAsync(Guid id, CancellationToken ct = default);
    Task<KpiAssignmentDto> CreateAssignmentAsync(CreateKpiAssignmentRequest req, CancellationToken ct = default);
    Task<KpiAssignmentDto> UpdateAssignmentAsync(Guid id, UpdateKpiAssignmentRequest req, CancellationToken ct = default);
    Task<KpiAssignmentDto> EvaluateAssignmentAsync(Guid id, EvaluateKpiRequest req, CancellationToken ct = default);
    Task<KpiAssignmentDto> ApproveAssignmentAsync(Guid id, ApproveKpiRequest req, CancellationToken ct = default);
    Task<KpiAssignmentDto> AutoCalculateActualsAsync(Guid id, CancellationToken ct = default);
}
