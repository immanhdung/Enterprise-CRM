using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Quotations;

public record QuotationItemDto(
    Guid Id,
    Guid QuotationId,
    Guid? ServiceId,
    string ServiceCode,
    string ServiceName,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal DiscountAmount,
    decimal VatRate,
    decimal VatAmount,
    decimal LineTotal,
    string? Notes
);

public record QuotationApprovalHistoryDto(
    Guid Id,
    Guid QuotationId,
    string Action,
    Guid ActorUserId,
    string ActorName,
    string? Comments,
    DateTime Timestamp
);

public record QuotationDto(
    Guid Id,
    string QuotationNumber,
    int Version,
    string RevisionNumber,
    Guid? ParentQuotationId,
    Guid? DealId,
    string? DealTitle,
    Guid AccountId,
    string AccountName,
    Guid? ContactId,
    string? ContactName,
    string? ContactEmail,
    Guid? PriceListId,
    string? PriceListName,
    DateTime IssueDate,
    DateTime ValidUntil,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal TotalAmount,
    string Status,
    string? TermsAndConditions,
    string? Notes,
    string? PdfUrl,
    DateTime? SubmittedAt,
    string? SubmittedByName,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    string? ApprovalNotes,
    string? RejectedByName,
    DateTime? RejectedAt,
    string? RejectionReason,
    DateTime? SentAt,
    string? SentToEmail,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<QuotationItemDto>? Items = null,
    IReadOnlyList<QuotationApprovalHistoryDto>? ApprovalHistories = null
);

public record QuotationItemInput(
    Guid? ServiceId,
    string ServiceCode,
    string ServiceName,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountRate = 0,
    decimal VatRate = 10,
    string? Notes = null
);

public record CreateQuotationRequest(
    Guid? DealId,
    Guid AccountId,
    Guid? ContactId,
    Guid? PriceListId,
    DateTime ValidUntil,
    string? TermsAndConditions,
    string? Notes,
    IReadOnlyList<QuotationItemInput> Items
);

public record UpdateQuotationRequest(
    Guid? ContactId,
    Guid? PriceListId,
    DateTime ValidUntil,
    string? TermsAndConditions,
    string? Notes,
    IReadOnlyList<QuotationItemInput> Items
);

public record SubmitQuotationApprovalRequest(
    string? Notes = null
);

public record ApproveQuotationRequest(
    string? ApprovalNotes = null
);

public record RejectQuotationRequest(
    string Reason
);

public record CreateRevisionRequest(
    string? ChangeNotes = null
);

public record SendQuotationEmailRequest(
    string RecipientEmail,
    string? Subject = null,
    string? CustomMessage = null
);

public record QuotationFilterParams(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? Status = null,
    Guid? AccountId = null,
    Guid? DealId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
);

public record PagedQuotationResult(
    IReadOnlyList<QuotationDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
