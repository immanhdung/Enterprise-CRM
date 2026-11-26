using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/workflows")]
public class WorkflowsController : BaseApiController
{
    private readonly IWorkflowEngine _workflowEngine;

    public WorkflowsController(IWorkflowEngine workflowEngine)
    {
        _workflowEngine = workflowEngine;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WorkflowDefinitionDto>>>> GetWorkflows(
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await _workflowEngine.GetWorkflowsAsync(category, isActive, ct);
        return OkResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> GetWorkflowById(Guid id, CancellationToken ct = default)
    {
        var result = await _workflowEngine.GetWorkflowByIdAsync(id, ct);
        if (result == null) return FailResponse<WorkflowDefinitionDto>("Workflow definition not found.", StatusCodes.Status404NotFound);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> CreateWorkflow(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken ct = default)
    {
        var result = await _workflowEngine.CreateWorkflowAsync(request, ct);
        return OkResponse(result, "Workflow definition created successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> UpdateWorkflow(
        Guid id,
        [FromBody] UpdateWorkflowDefinitionRequest request,
        CancellationToken ct = default)
    {
        var result = await _workflowEngine.UpdateWorkflowAsync(id, request, ct);
        return OkResponse(result, "Workflow definition updated successfully.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkflow(Guid id, CancellationToken ct = default)
    {
        var success = await _workflowEngine.DeleteWorkflowAsync(id, ct);
        return OkResponse(success, "Workflow definition deleted.");
    }

    [HttpPost("trigger")]
    public async Task<ActionResult<ApiResponse<WorkflowExecutionDto>>> TriggerWorkflow(
        [FromBody] ExecuteWorkflowRequest request,
        CancellationToken ct = default)
    {
        var result = await _workflowEngine.TriggerWorkflowAsync(request, ct);
        return OkResponse(result, "Workflow executed.");
    }

    [HttpGet("executions")]
    public async Task<ActionResult<ApiResponse<List<WorkflowExecutionDto>>>> GetExecutions(
        [FromQuery] Guid? workflowId = null,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var result = await _workflowEngine.GetExecutionsAsync(workflowId, limit, ct);
        return OkResponse(result);
    }

    [HttpGet("executions/{id}")]
    public async Task<ActionResult<ApiResponse<WorkflowExecutionDto>>> GetExecutionById(Guid id, CancellationToken ct = default)
    {
        var result = await _workflowEngine.GetExecutionByIdAsync(id, ct);
        if (result == null) return FailResponse<WorkflowExecutionDto>("Execution record not found.", StatusCodes.Status404NotFound);
        return OkResponse(result);
    }
}
