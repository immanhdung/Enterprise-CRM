using System.Diagnostics;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly IApplicationDbContext _context;

    public WorkflowEngine(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowDefinitionDto>> GetWorkflowsAsync(string? category = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowDefinitions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(w => w.Category == category);
        }

        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        var list = await query.OrderBy(w => w.ExecutionOrder).ThenByDescending(w => w.CreatedAt).ToListAsync(cancellationToken);

        return list.Select(w => new WorkflowDefinitionDto(
            w.Id,
            w.Name,
            w.Description,
            w.Category,
            w.TriggerEvent,
            w.ConditionsJson,
            w.ActionsJson,
            w.ExecutionOrder,
            w.Version,
            w.IsActive,
            w.ExecutionCount,
            w.ErrorCount,
            w.LastExecutedAt,
            w.CreatedAt
        )).ToList();
    }

    public async Task<WorkflowDefinitionDto?> GetWorkflowByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var w = await _context.WorkflowDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (w == null) return null;

        return new WorkflowDefinitionDto(
            w.Id,
            w.Name,
            w.Description,
            w.Category,
            w.TriggerEvent,
            w.ConditionsJson,
            w.ActionsJson,
            w.ExecutionOrder,
            w.Version,
            w.IsActive,
            w.ExecutionCount,
            w.ErrorCount,
            w.LastExecutedAt,
            w.CreatedAt
        );
    }

    public async Task<WorkflowDefinitionDto> CreateWorkflowAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var workflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = request.Category,
            TriggerEvent = request.TriggerEvent,
            ConditionsJson = request.ConditionsJson,
            ActionsJson = request.ActionsJson,
            ExecutionOrder = request.ExecutionOrder,
            Version = 1,
            IsActive = request.IsActive,
            ExecutionCount = 0,
            ErrorCount = 0
        };

        _context.WorkflowDefinitions.Add(workflow);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetWorkflowByIdAsync(workflow.Id, cancellationToken))!;
    }

    public async Task<WorkflowDefinitionDto> UpdateWorkflowAsync(Guid id, UpdateWorkflowDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var workflow = await _context.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (workflow == null)
            throw new KeyNotFoundException($"Workflow definition {id} not found.");

        workflow.Name = request.Name.Trim();
        workflow.Description = request.Description?.Trim() ?? string.Empty;
        workflow.Category = request.Category;
        workflow.TriggerEvent = request.TriggerEvent;
        workflow.ConditionsJson = request.ConditionsJson;
        workflow.ActionsJson = request.ActionsJson;
        workflow.ExecutionOrder = request.ExecutionOrder;
        workflow.IsActive = request.IsActive;
        workflow.Version += 1;

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetWorkflowByIdAsync(workflow.Id, cancellationToken))!;
    }

    public async Task<bool> DeleteWorkflowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflow = await _context.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (workflow == null) return false;

        workflow.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<WorkflowExecutionDto> TriggerWorkflowAsync(ExecuteWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var workflows = await _context.WorkflowDefinitions
            .Where(w => w.IsActive && w.TriggerEvent == request.TriggerEvent)
            .OrderBy(w => w.ExecutionOrder)
            .ToListAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var mainWorkflow = workflows.FirstOrDefault();

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            WorkflowId = mainWorkflow?.Id ?? Guid.Empty,
            TriggerEvent = request.TriggerEvent,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Status = "Success",
            ExecutedAt = DateTime.UtcNow,
            DurationMs = 0,
            ResultSummary = "No matching active workflow"
        };

        if (mainWorkflow == null)
        {
            stopwatch.Stop();
            execution.DurationMs = stopwatch.ElapsedMilliseconds;
            return MapExecutionDto(execution, new List<WorkflowExecutionLogDto>());
        }

        var executionLogs = new List<WorkflowExecutionLog>();

        try
        {
            // Evaluate conditions & execute actions
            bool conditionPassed = EvaluateConditions(mainWorkflow.ConditionsJson, request.ContextData);

            if (!conditionPassed)
            {
                execution.Status = "Skipped";
                execution.ResultSummary = "Conditions evaluated to false";

                executionLogs.Add(new WorkflowExecutionLog
                {
                    Id = Guid.NewGuid(),
                    WorkflowExecutionId = execution.Id,
                    StepName = "Condition Check",
                    ActionType = "EvaluateConditions",
                    Status = "Skipped",
                    InputJson = mainWorkflow.ConditionsJson,
                    OutputJson = "false",
                    ExecutedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Execute actions defined in ActionsJson
                var actionLogs = await ExecuteActionsAsync(execution.Id, mainWorkflow.ActionsJson, request, cancellationToken);
                executionLogs.AddRange(actionLogs);

                execution.Status = actionLogs.Any(l => l.Status == "Failed") ? "Partial" : "Success";
                execution.ResultSummary = $"Executed {actionLogs.Count} action(s).";
            }

            mainWorkflow.ExecutionCount += 1;
            mainWorkflow.LastExecutedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            execution.Status = "Failed";
            execution.ErrorMessage = ex.Message;
            mainWorkflow.ErrorCount += 1;

            executionLogs.Add(new WorkflowExecutionLog
            {
                Id = Guid.NewGuid(),
                WorkflowExecutionId = execution.Id,
                StepName = "Execution Error",
                ActionType = "ErrorHandling",
                Status = "Failed",
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow
            });
        }
        finally
        {
            stopwatch.Stop();
            execution.DurationMs = stopwatch.ElapsedMilliseconds;
        }

        _context.WorkflowExecutions.Add(execution);
        foreach (var log in executionLogs)
        {
            _context.WorkflowExecutionLogs.Add(log);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapExecutionDto(execution, executionLogs.Select(l => new WorkflowExecutionLogDto(
            l.Id,
            l.WorkflowExecutionId,
            l.StepName,
            l.ActionType,
            l.Status,
            l.InputJson,
            l.OutputJson,
            l.ErrorMessage,
            l.ExecutedAt
        )).ToList(), mainWorkflow.Name);
    }

    public async Task<List<WorkflowExecutionDto>> GetExecutionsAsync(Guid? workflowId = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowExecutions
            .Include(e => e.Workflow)
            .Include(e => e.Logs)
            .AsNoTracking()
            .AsQueryable();

        if (workflowId.HasValue)
        {
            query = query.Where(e => e.WorkflowId == workflowId.Value);
        }

        var list = await query.OrderByDescending(e => e.ExecutedAt).Take(limit).ToListAsync(cancellationToken);

        return list.Select(e => MapExecutionDto(e, e.Logs.Select(l => new WorkflowExecutionLogDto(
            l.Id,
            l.WorkflowExecutionId,
            l.StepName,
            l.ActionType,
            l.Status,
            l.InputJson,
            l.OutputJson,
            l.ErrorMessage,
            l.ExecutedAt
        )).ToList(), e.Workflow?.Name ?? "Unknown")).ToList();
    }

    public async Task<WorkflowExecutionDto?> GetExecutionByIdAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        var e = await _context.WorkflowExecutions
            .Include(x => x.Workflow)
            .Include(x => x.Logs)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == executionId, cancellationToken);

        if (e == null) return null;

        return MapExecutionDto(e, e.Logs.Select(l => new WorkflowExecutionLogDto(
            l.Id,
            l.WorkflowExecutionId,
            l.StepName,
            l.ActionType,
            l.Status,
            l.InputJson,
            l.OutputJson,
            l.ErrorMessage,
            l.ExecutedAt
        )).ToList(), e.Workflow?.Name ?? "Unknown");
    }

    private bool EvaluateConditions(string? conditionsJson, Dictionary<string, object>? context)
    {
        if (string.IsNullOrWhiteSpace(conditionsJson) || conditionsJson == "[]" || conditionsJson == "{}")
            return true;

        try
        {
            using var doc = JsonDocument.Parse(conditionsJson);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var condition in root.EnumerateArray())
                {
                    if (!EvaluateSingleCondition(condition, context))
                        return false;
                }
                return true;
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                return EvaluateSingleCondition(root, context);
            }

            return true;
        }
        catch
        {
            // If condition parsing fails, default to true to allow processing or log
            return true;
        }
    }

    private bool EvaluateSingleCondition(JsonElement condition, Dictionary<string, object>? context)
    {
        if (context == null) return true;

        if (!condition.TryGetProperty("field", out var fieldElem)) return true;
        var field = fieldElem.GetString();
        if (string.IsNullOrEmpty(field) || !context.TryGetValue(field, out var contextVal))
            return true;

        var op = condition.TryGetProperty("operator", out var opElem) ? opElem.GetString() : "Equals";
        var expectedVal = condition.TryGetProperty("value", out var valElem) ? valElem.ToString() : null;

        var actualStr = contextVal?.ToString();

        return op switch
        {
            "Equals" => string.Equals(actualStr, expectedVal, StringComparison.OrdinalIgnoreCase),
            "NotEquals" => !string.Equals(actualStr, expectedVal, StringComparison.OrdinalIgnoreCase),
            "Contains" => actualStr?.Contains(expectedVal ?? "", StringComparison.OrdinalIgnoreCase) ?? false,
            "GreaterThan" => decimal.TryParse(actualStr, out var aNum) && decimal.TryParse(expectedVal, out var eNum) && aNum > eNum,
            "LessThan" => decimal.TryParse(actualStr, out var lNum) && decimal.TryParse(expectedVal, out var leNum) && lNum < leNum,
            _ => true
        };
    }

    private async Task<List<WorkflowExecutionLog>> ExecuteActionsAsync(
        Guid executionId,
        string? actionsJson,
        ExecuteWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var logs = new List<WorkflowExecutionLog>();
        if (string.IsNullOrWhiteSpace(actionsJson))
            return logs;

        try
        {
            using var doc = JsonDocument.Parse(actionsJson);
            var root = doc.RootElement;

            var actions = root.ValueKind == JsonValueKind.Array ? root.EnumerateArray().ToList() : new List<JsonElement> { root };

            foreach (var action in actions)
            {
                var actionType = action.TryGetProperty("type", out var typeElem) ? typeElem.GetString() ?? "Unknown" : "Unknown";
                var stepName = action.TryGetProperty("name", out var nameElem) ? nameElem.GetString() ?? actionType : actionType;

                var log = new WorkflowExecutionLog
                {
                    Id = Guid.NewGuid(),
                    WorkflowExecutionId = executionId,
                    StepName = stepName,
                    ActionType = actionType,
                    InputJson = action.GetRawText(),
                    ExecutedAt = DateTime.UtcNow
                };

                try
                {
                    switch (actionType)
                    {
                        case "SendNotification":
                            var title = action.TryGetProperty("title", out var tElem) ? tElem.GetString() : "Workflow Notification";
                            var message = action.TryGetProperty("message", out var mElem) ? mElem.GetString() : $"Workflow action for {request.EntityType} {request.EntityId}";
                            
                            // Get first admin or sales user if not specified
                            var user = await _context.Users.FirstOrDefaultAsync(cancellationToken);
                            if (user != null)
                            {
                                var notification = new Notification
                                {
                                    Id = Guid.NewGuid(),
                                    RecipientUserId = user.Id,
                                    Title = title ?? "Workflow Alert",
                                    Message = message ?? "",
                                    Type = "Workflow",
                                    Category = "Workflow",
                                    Priority = "Normal",
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.Notifications.Add(notification);
                            }
                            log.Status = "Success";
                            log.OutputJson = "{\"notificationSent\": true}";
                            break;

                        case "CreateTask":
                            var taskTitle = action.TryGetProperty("taskTitle", out var ttElem) ? ttElem.GetString() : "Automated Workflow Task";
                            var task = new TaskItem
                            {
                                Id = Guid.NewGuid(),
                                Title = taskTitle ?? "Automated Follow-up Task",
                                Priority = "High",
                                Status = "Pending",
                                DueDate = DateTime.UtcNow.AddDays(2),
                                LeadId = request.EntityType == "Lead" ? request.EntityId : null,
                                DealId = request.EntityType == "Deal" ? request.EntityId : null,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Tasks.Add(task);
                            log.Status = "Success";
                            log.OutputJson = $"{{\"taskId\": \"{task.Id}\"}}";
                            break;

                        default:
                            log.Status = "Success";
                            log.OutputJson = "{\"status\": \"processed\"}";
                            break;
                    }
                }
                catch (Exception actionEx)
                {
                    log.Status = "Failed";
                    log.ErrorMessage = actionEx.Message;
                }

                logs.Add(log);
            }
        }
        catch (Exception ex)
        {
            logs.Add(new WorkflowExecutionLog
            {
                Id = Guid.NewGuid(),
                WorkflowExecutionId = executionId,
                StepName = "Action Parsing",
                ActionType = "Parse",
                Status = "Failed",
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow
            });
        }

        return logs;
    }

    private static WorkflowExecutionDto MapExecutionDto(WorkflowExecution e, List<WorkflowExecutionLogDto> logs, string workflowName = "")
    {
        return new WorkflowExecutionDto(
            e.Id,
            e.WorkflowId,
            string.IsNullOrEmpty(workflowName) ? (e.Workflow?.Name ?? "Workflow") : workflowName,
            e.TriggerEvent,
            e.EntityType,
            e.EntityId,
            e.Status,
            e.ExecutedAt,
            e.DurationMs,
            e.ResultSummary,
            e.ErrorMessage,
            logs
        );
    }
}
