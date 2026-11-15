using EnterpriseCRM.Application.Features.Contacts;
using EnterpriseCRM.Application.Features.Deals;
using EnterpriseCRM.Application.Features.TasksAndActivities;

namespace EnterpriseCRM.Application.Features.Accounts;

public record AccountDto(
    Guid Id,
    string Code,
    string Name,
    string? TaxCode,
    string? Industry,
    string? Website,
    string? Phone,
    string? Email,
    string? BillingAddress,
    string Classification, // VIP, Standard, Prospect, Partner
    string Status, // Active, Inactive, Suspended
    string? Description,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    int ContactCount,
    int DealCount,
    decimal TotalDealValue,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AccountDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? TaxCode,
    string? Industry,
    string? Website,
    string? Phone,
    string? Email,
    string? BillingAddress,
    string Classification,
    string Status,
    string? Description,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    IReadOnlyList<ContactDto> Contacts,
    IReadOnlyList<DealDto> Deals,
    IReadOnlyList<TimelineEventDto> Timeline,
    IReadOnlyList<AccountContractSummaryDto> Contracts,
    IReadOnlyList<AccountProjectSummaryDto> Projects,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AccountContractSummaryDto(
    Guid Id,
    string ContractNumber,
    string Title,
    decimal TotalAmount,
    string Status,
    DateOnly StartDate,
    DateOnly EndDate
);

public record AccountProjectSummaryDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    decimal ProgressPercentage
);

public record CreateAccountRequest(
    string Name,
    string? TaxCode,
    string? Industry,
    string? Website,
    string? Phone,
    string? Email,
    string? BillingAddress,
    string? Classification,
    string? Status,
    string? Description,
    Guid? AssignedEmployeeId
);

public record UpdateAccountRequest(
    string Name,
    string? TaxCode,
    string? Industry,
    string? Website,
    string? Phone,
    string? Email,
    string? BillingAddress,
    string Classification,
    string Status,
    string? Description,
    Guid? AssignedEmployeeId
);
