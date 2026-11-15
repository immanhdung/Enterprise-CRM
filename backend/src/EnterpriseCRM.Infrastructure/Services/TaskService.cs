using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<TaskService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedList<TaskDto>>> GetTasksAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .Include(t => t.Deal)
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .Include(t => t.Lead)
            .AsQueryable();

        if (myTasksOnly == true && _currentUserService.UserId.HasValue)
        {
            var myEmpId = await _context.Employees
                .Where(e => e.UserId == _currentUserService.UserId.Value)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken);

            query = query.Where(t => t.AssignedEmployeeId == myEmpId);
        }
        else if (assignedEmployeeId.HasValue)
        {
            query = query.Where(t => t.AssignedEmployeeId == assignedEmployeeId.Value);
        }

        if (overdueOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled");
        }

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(t => t.Status == status);
        if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(t => t.Priority == priority);
        if (dealId.HasValue) query = query.Where(t => t.DealId == dealId.Value);
        if (accountId.HasValue) query = query.Where(t => t.AccountId == accountId.Value);
        if (contactId.HasValue) query = query.Where(t => t.ContactId == contactId.Value);
        if (leadId.HasValue) query = query.Where(t => t.LeadId == leadId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(s) ||
                (t.Description != null && t.Description.ToLower().Contains(s)));
        }

        var count = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageIndex);
        var size = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.Status,
                t.StartDate,
                t.DueDate,
                t.CompletedAt,
                t.AssignedEmployeeId,
                t.AssignedEmployee != null ? t.AssignedEmployee.User.FullName : null,
                t.DealId,
                t.Deal != null ? t.Deal.Title : null,
                t.AccountId,
                t.Account != null ? t.Account.Name : null,
                t.ContactId,
                t.Contact != null ? t.Contact.FullName : null,
                t.LeadId,
                t.Lead != null ? t.Lead.FullName : null,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<PaginatedList<TaskDto>>.SuccessResponse(
            new PaginatedList<TaskDto>(items, count, page, size)
        );
    }

    public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var t = await _context.Tasks
            .Include(x => x.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .Include(x => x.Deal)
            .Include(x => x.Account)
            .Include(x => x.Contact)
            .Include(x => x.Lead)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (t == null)
        {
            return ApiResponse<TaskDto>.FailureResponse("Task not found.", 404);
        }

        var dto = new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Priority,
            t.Status,
            t.StartDate,
            t.DueDate,
            t.CompletedAt,
            t.AssignedEmployeeId,
            t.AssignedEmployee?.User?.FullName,
            t.DealId,
            t.Deal?.Title,
            t.AccountId,
            t.Account?.Name,
            t.ContactId,
            t.Contact?.FullName,
            t.LeadId,
            t.Lead?.FullName,
            t.CreatedAt,
            t.UpdatedAt
        );

        return ApiResponse<TaskDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority ?? "Medium",
            Status = "Pending",
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            AssignedEmployeeId = request.AssignedEmployeeId,
            DealId = request.DealId,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            LeadId = request.LeadId
        };

        _context.Tasks.Add(task);

        // Record Timeline Activity
        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            LeadId = request.LeadId,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            DealId = request.DealId,
            ActivityType = "TaskCompleted",
            Subject = $"Task Created: '{task.Title}'",
            Notes = $"Due by {task.DueDate:yyyy-MM-dd HH:mm}",
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "User",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created Task {TaskId} ('{Title}')", task.Id, task.Title);

        return await GetTaskByIdAsync(task.Id, cancellationToken);
    }

    public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null)
        {
            return ApiResponse<TaskDto>.FailureResponse("Task not found.", 404);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.StartDate = request.StartDate;
        task.DueDate = request.DueDate;
        task.CompletedAt = request.Status == "Completed" ? (request.CompletedAt ?? DateTime.UtcNow) : null;
        task.AssignedEmployeeId = request.AssignedEmployeeId;
        task.DealId = request.DealId;
        task.AccountId = request.AccountId;
        task.ContactId = request.ContactId;
        task.LeadId = request.LeadId;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetTaskByIdAsync(task.Id, cancellationToken);
    }

    public async Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null)
        {
            return ApiResponse<TaskDto>.FailureResponse("Task not found.", 404);
        }

        task.Status = status;
        if (status == "Completed")
        {
            task.CompletedAt = DateTime.UtcNow;

            _context.CrmActivities.Add(new CrmActivity
            {
                Id = Guid.NewGuid(),
                LeadId = task.LeadId,
                AccountId = task.AccountId,
                ContactId = task.ContactId,
                DealId = task.DealId,
                ActivityType = "TaskCompleted",
                Subject = $"Task Completed: '{task.Title}'",
                PerformedByUserId = _currentUserService.UserId,
                PerformedByName = _currentUserService.Email ?? "User",
                CompletedAt = DateTime.UtcNow
            });
        }
        else
        {
            task.CompletedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetTaskByIdAsync(task.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null)
        {
            return ApiResponse<bool>.FailureResponse("Task not found.", 404);
        }

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        task.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully.");
    }
}
