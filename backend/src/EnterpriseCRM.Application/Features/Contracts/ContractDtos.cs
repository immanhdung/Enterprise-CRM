using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Contracts;

public record ContractItemDto(
    Guid Id,
    Guid ContractId,
    Guid? ServiceId,
    string ServiceName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Notes
);

public record ContractAppendixDto(
    Guid Id,
    Guid ContractId,
    string AppendixNumber,
    string Title,
    string? Description,
    decimal AdjustedValue,
    DateTime EffectiveDate,
    string Status,
    string? DocumentUrl,
    DateTime CreatedAt
);

public record ContractHistoryDto(
    Guid Id,
    Guid ContractId,
    string Action,
    string? OldStatus,
    string? NewStatus,
    Guid ActorUserId,
    string ActorName,
    string? Notes,
    DateTime Timestamp
);

public record ContractDto(
    Guid Id,
    string ContractNumber,
    string Title,
    Guid AccountId,
    string AccountName,
    Guid? ContactId,
    string? ContactName,
    string? ContactEmail,
    Guid? QuotationId,
    string? QuotationNumber,
    Guid? TemplateId,
    string? TemplateName,
    Guid? ParentContractId,
    string? ParentContractNumber,
    int RenewalCount,
    decimal TotalValue,
    ContractStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    string? ScopeOfWork,
    string? PaymentTerms,
    string? SpecialTerms,
    string? DocumentUrl,
    DateTime? SignedAt,
    string? SignedByClientName,
    string? SignedByClientEmail,
    string? SignerIpAddress,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ContractItemDto>? Items = null,
    IReadOnlyList<ContractAppendixDto>? Appendices = null,
    IReadOnlyList<ContractHistoryDto>? Histories = null
);

public record ContractItemInput(
    Guid? ServiceId,
    string ServiceName,
    int Quantity,
    decimal UnitPrice,
    string? Notes = null
);

public record CreateContractRequest(
    string Title,
    Guid AccountId,
    Guid? ContactId,
    Guid? QuotationId,
    Guid? TemplateId,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalValue,
    string? ScopeOfWork,
    string? PaymentTerms,
    string? SpecialTerms,
    IReadOnlyList<ContractItemInput>? Items = null
);

public record GenerateContractFromQuotationRequest(
    Guid QuotationId,
    Guid? TemplateId,
    string? Title,
    DateTime StartDate,
    DateTime EndDate,
    string? PaymentTerms,
    string? SpecialTerms
);

public record UpdateContractRequest(
    string Title,
    Guid? ContactId,
    ContractStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalValue,
    string? ScopeOfWork,
    string? PaymentTerms,
    string? SpecialTerms,
    IReadOnlyList<ContractItemInput>? Items = null
);

public record CreateContractAppendixRequest(
    string Title,
    string? Description,
    decimal AdjustedValue,
    DateTime EffectiveDate,
    string Status = "Draft"
);

public record RenewContractRequest(
    DateTime NewStartDate,
    DateTime NewEndDate,
    decimal? NewTotalValue,
    string? RenewalNotes
);

public record ContractTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string? Description,
    string Content,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateContractTemplateRequest(
    string Code,
    string Name,
    string Category,
    string? Description,
    string Content,
    bool IsDefault = false,
    bool IsActive = true
);

public record UpdateContractTemplateRequest(
    string Name,
    string Category,
    string? Description,
    string Content,
    bool IsDefault,
    bool IsActive
);

public record ContractFilterParams(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    ContractStatus? Status = null,
    Guid? AccountId = null,
    DateTime? StartDateFrom = null,
    DateTime? EndDateTo = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
);

public record PagedContractResult(
    IReadOnlyList<ContractDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
