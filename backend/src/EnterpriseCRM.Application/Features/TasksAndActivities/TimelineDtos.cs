namespace EnterpriseCRM.Application.Features.TasksAndActivities;

public record TimelineEventDto(
    Guid Id,
    string ActivityType, // Call, Email, Meeting, Note, StatusChange, StageChange, Assignment, TaskCompleted, DealConverted
    string Subject,
    string? Notes,
    string? PerformedByName,
    Guid? PerformedByUserId,
    DateTime? ScheduledAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    Guid? LeadId,
    Guid? AccountId,
    Guid? ContactId,
    Guid? DealId
);

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Priority, // Low, Medium, High, Urgent
    string Status, // Pending, InProgress, Completed, Overdue, Cancelled
    DateTime? StartDate,
    DateTime DueDate,
    DateTime? CompletedAt,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    Guid? DealId,
    string? DealTitle,
    Guid? AccountId,
    string? AccountName,
    Guid? ContactId,
    string? ContactName,
    Guid? LeadId,
    string? LeadName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    DateTime? StartDate,
    DateTime DueDate,
    Guid? AssignedEmployeeId,
    Guid? DealId,
    Guid? AccountId,
    Guid? ContactId,
    Guid? LeadId
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    string Status,
    DateTime? StartDate,
    DateTime DueDate,
    DateTime? CompletedAt,
    Guid? AssignedEmployeeId,
    Guid? DealId,
    Guid? AccountId,
    Guid? ContactId,
    Guid? LeadId
);

public record AppointmentDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string? Location,
    string? MeetingLink,
    string Status, // Scheduled, Completed, Cancelled, Rescheduled
    Guid EmployeeId,
    string EmployeeName,
    Guid? DealId,
    string? DealTitle,
    Guid? AccountId,
    string? AccountName,
    Guid? ContactId,
    string? ContactName,
    Guid? LeadId,
    string? LeadName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateAppointmentRequest(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string? Location,
    string? MeetingLink,
    Guid EmployeeId,
    Guid? DealId,
    Guid? AccountId,
    Guid? ContactId,
    Guid? LeadId
);

public record UpdateAppointmentRequest(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string? Location,
    string? MeetingLink,
    string Status,
    Guid EmployeeId,
    Guid? DealId,
    Guid? AccountId,
    Guid? ContactId,
    Guid? LeadId
);
