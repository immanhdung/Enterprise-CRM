namespace EnterpriseCRM.Application.Features.AutomationAndAi;

public interface ILeadAssignmentEngine
{
    Task<List<LeadAssignmentRuleDto>> GetRulesAsync(CancellationToken cancellationToken = default);
    Task<LeadAssignmentRuleDto> CreateRuleAsync(CreateLeadAssignmentRuleRequest request, CancellationToken cancellationToken = default);
    Task<LeadAssignmentRuleDto> UpdateRuleAsync(Guid id, CreateLeadAssignmentRuleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LeadAssignmentLogDto> AssignLeadAsync(AssignLeadRequest request, CancellationToken cancellationToken = default);
    Task<List<LeadAssignmentLogDto>> GetAssignmentLogsAsync(Guid? leadId = null, int limit = 50, CancellationToken cancellationToken = default);
    Task<LeadDistributionMetricsDto> GetDistributionMetricsAsync(CancellationToken cancellationToken = default);
}
