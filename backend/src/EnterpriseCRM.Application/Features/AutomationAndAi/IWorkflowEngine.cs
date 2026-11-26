namespace EnterpriseCRM.Application.Features.AutomationAndAi;

public interface IWorkflowEngine
{
    Task<List<WorkflowDefinitionDto>> GetWorkflowsAsync(string? category = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionDto?> GetWorkflowByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionDto> CreateWorkflowAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionDto> UpdateWorkflowAsync(Guid id, UpdateWorkflowDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkflowAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkflowExecutionDto> TriggerWorkflowAsync(ExecuteWorkflowRequest request, CancellationToken cancellationToken = default);
    Task<List<WorkflowExecutionDto>> GetExecutionsAsync(Guid? workflowId = null, int limit = 50, CancellationToken cancellationToken = default);
    Task<WorkflowExecutionDto?> GetExecutionByIdAsync(Guid executionId, CancellationToken cancellationToken = default);
}
