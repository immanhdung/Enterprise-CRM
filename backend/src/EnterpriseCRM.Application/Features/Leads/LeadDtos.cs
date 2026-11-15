using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Leads;

public record LeadDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? CompanyName,
    string Phone,
    string? Email,
    string? JobTitle,
    decimal EstimatedBudget,
    LeadStatus Status,
    int Score,
    bool IsKeyAccount,
    string Priority,
    string? Campaign,
    DateTime? NextFollowUpDate,
    string? Description,
    string? Notes,
    Guid? SourceId,
    string? SourceName,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    Guid? ConvertedAccountId,
    Guid? ConvertedContactId,
    Guid? ConvertedDealId,
    DateTime? ConvertedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateLeadRequest(
    string FirstName,
    string LastName,
    string? CompanyName,
    string Phone,
    string? Email,
    string? JobTitle,
    decimal EstimatedBudget,
    Guid? SourceId,
    Guid? AssignedEmployeeId,
    string? Priority,
    string? Campaign,
    DateTime? NextFollowUpDate,
    string? Description,
    string? Notes
);

public record UpdateLeadRequest(
    string FirstName,
    string LastName,
    string? CompanyName,
    string Phone,
    string? Email,
    string? JobTitle,
    decimal EstimatedBudget,
    LeadStatus Status,
    int Score,
    bool IsKeyAccount,
    Guid? SourceId,
    Guid? AssignedEmployeeId,
    string Priority,
    string? Campaign,
    DateTime? NextFollowUpDate,
    string? Description,
    string? Notes
);

public record AssignLeadRequest(
    Guid? EmployeeId,
    string Mode, // "Manual", "RoundRobin", "Workload"
    string? Notes
);

public record ConvertLeadRequest(
    string? AccountName,
    Guid? ExistingAccountId,
    string? ContactFirstName,
    string? ContactLastName,
    Guid? ExistingContactId,
    string DealTitle,
    decimal DealAmount,
    DateOnly ExpectedClosingDate,
    Guid? PipelineStageId,
    string? Notes
);

public record ConvertLeadResult(
    Guid AccountId,
    string AccountName,
    Guid ContactId,
    string ContactName,
    Guid DealId,
    string DealTitle,
    DateTime ConvertedAt
);

public record DuplicateCheckRequest(
    string Phone,
    string? Email
);

public record DuplicateCheckResult(
    bool IsDuplicate,
    string? Reason,
    Guid? ExistingLeadId,
    string? ExistingLeadName,
    string? ExistingLeadStatus
);

public record LeadActivityDto(
    Guid Id,
    Guid LeadId,
    string ActivityType,
    string Subject,
    string? Notes,
    Guid? EmployeeId,
    string? EmployeeName,
    DateTime? ScheduledAt,
    DateTime? CompletedAt,
    DateTime CreatedAt
);

public record CreateLeadActivityRequest(
    string ActivityType,
    string Subject,
    string? Notes,
    DateTime? ScheduledAt,
    DateTime? CompletedAt
);

public record LeadImportItem(
    string FirstName,
    string LastName,
    string? CompanyName,
    string Phone,
    string? Email,
    string? JobTitle,
    decimal EstimatedBudget,
    string? SourceName,
    string? Priority,
    string? Notes
);

public record LeadImportResult(
    int TotalProcessed,
    int SuccessCount,
    int DuplicateCount,
    int ErrorCount,
    List<string> ErrorMessages
);
