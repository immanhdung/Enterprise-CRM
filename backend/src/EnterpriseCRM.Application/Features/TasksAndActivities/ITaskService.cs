using EnterpriseCRM.Application.Common.Models;

namespace EnterpriseCRM.Application.Features.TasksAndActivities;

public interface ITaskService
{
    Task<ApiResponse<PaginatedList<TaskDto>>> GetTasksAsync(
        string? search,
        string? status,
        string? priority,
        Guid? assignedEmployeeId,
        bool? myTasksOnly,
        bool? overdueOnly,
        Guid? dealId,
        Guid? accountId,
        Guid? contactId,
        Guid? leadId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskDto>> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
}
