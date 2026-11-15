using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<TaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] Guid? assignedEmployeeId,
        [FromQuery] bool? myTasksOnly,
        [FromQuery] bool? overdueOnly,
        [FromQuery] Guid? dealId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? contactId,
        [FromQuery] Guid? leadId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _taskService.GetTasksAsync(
            search, status, priority, assignedEmployeeId, myTasksOnly, overdueOnly,
            dealId, accountId, contactId, leadId, pageIndex, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTaskByIdAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.LeadsCreate)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateTaskAsync(request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateTaskAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromQuery] string status, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateTaskStatusAsync(id, status, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.LeadsDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteTaskAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}
