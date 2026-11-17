using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Finance;

// ==========================================
// 1. PAYMENT SCHEDULE DTOs
// ==========================================

public record PaymentScheduleDto(
    Guid Id,
    Guid ContractId,
    string? ContractNumber,
    Guid? AccountId,
    string? AccountName,
    int InstallmentNumber,
    string? MilestoneTitle,
    decimal Percentage,
    DateOnly DueDate,
    decimal Amount,
    decimal PaidAmount,
    string Status,
    string? Notes,
    DateTime? PaidAt,
    DateTime CreatedAt
);

public record CreatePaymentScheduleRequest(
    string MilestoneTitle,
    int InstallmentNumber,
    decimal Percentage,
    DateOnly DueDate,
    decimal Amount,
    string? Notes
);

public record InstallmentPlanItemInput(
    string MilestoneTitle,
    decimal Percentage,
    DateOnly DueDate,
    string? Notes = null
);

public record GenerateInstallmentSchedulesRequest(
    IReadOnlyList<InstallmentPlanItemInput> Installments
);

// ==========================================
// 2. PAYMENT DTOs
// ==========================================

public record PaymentDto(
    Guid Id,
    string PaymentNumber,
    Guid ContractId,
    string? ContractNumber,
    Guid AccountId,
    string? AccountName,
    Guid? PaymentScheduleId,
    string? MilestoneTitle,
    decimal Amount,
    DateTime PaymentDate,
    PaymentMethod Method,
    string? TransactionReference,
    string? BankName,
    string? BankAccountNumber,
    string? SenderName,
    string? Notes,
    string? AttachmentUrl,
    string Status,
    DateTime? ConfirmedAt,
    Guid? ConfirmedById,
    string? ConfirmedByName,
    DateTime? CancelledAt,
    Guid? CancelledById,
    string? ReversalReason,
    DateTime CreatedAt
);

public record CreatePaymentRequest(
    Guid ContractId,
    Guid? AccountId,
    Guid? PaymentScheduleId,
    decimal Amount,
    DateTime PaymentDate,
    PaymentMethod Method,
    string? TransactionReference,
    string? BankName,
    string? BankAccountNumber,
    string? SenderName,
    string? Notes,
    string? AttachmentUrl
);

public record ConfirmPaymentRequest(
    string? Notes
);

public record ReversePaymentRequest(
    string ReversalReason
);

public record PaymentFilterParams(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? Status = null,
    PaymentMethod? Method = null,
    Guid? ContractId = null,
    Guid? AccountId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
);

public record PagedPaymentResult(
    IReadOnlyList<PaymentDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

// ==========================================
// 3. CONTRACT FINANCIAL SUMMARY
// ==========================================

public record ContractFinancialSummaryDto(
    Guid ContractId,
    string ContractNumber,
    string ContractTitle,
    Guid AccountId,
    string AccountName,
    decimal TotalValue,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal RemainingContractValue,
    int TotalSchedulesCount,
    int PaidSchedulesCount,
    int OverdueSchedulesCount,
    IReadOnlyList<PaymentScheduleDto> Schedules,
    IReadOnlyList<PaymentDto> Payments
);

// ==========================================
// 4. INVOICE DTOs
// ==========================================

public record InvoiceItemDto(
    Guid Id,
    Guid InvoiceId,
    Guid? ServiceId,
    string? ServiceName,
    int LineNumber,
    string Description,
    string Unit,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal TaxAmount,
    decimal LineTotal
);

public record InvoiceItemInput(
    Guid? ServiceId,
    string Description,
    string? Unit,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate = 8m
);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string InvoiceSeries,
    Guid ContractId,
    string? ContractNumber,
    Guid AccountId,
    string? AccountName,
    string CustomerName,
    string? CustomerTaxCode,
    string? CustomerAddress,
    string? CustomerEmail,
    DateOnly IssueDate,
    decimal Subtotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal TotalAmount,
    InvoiceStatus Status,
    string? ElectronicInvoiceProvider,
    string? ElectronicInvoiceId,
    string? LookupCode,
    string? LookupUrl,
    string? ErrorMessage,
    int RetryCount,
    string? CancellationReason,
    DateTime? CancelledAt,
    DateTime CreatedAt,
    IReadOnlyList<InvoiceItemDto>? Items = null
);

public record IssueInvoiceRequest(
    Guid ContractId,
    Guid? AccountId,
    Guid? InvoiceRequestId,
    Guid? PaymentId,
    string CustomerName,
    string? CustomerTaxCode,
    string? CustomerAddress,
    string? CustomerEmail,
    DateOnly IssueDate,
    string? ElectronicInvoiceProvider,
    IReadOnlyList<InvoiceItemInput> Items
);

public record CancelInvoiceRequest(
    string CancellationReason
);

public record InvoiceFilterParams(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    InvoiceStatus? Status = null,
    string? ElectronicInvoiceProvider = null,
    Guid? ContractId = null,
    Guid? AccountId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
);

public record PagedInvoiceResult(
    IReadOnlyList<InvoiceDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

// ==========================================
// 5. INVOICE REQUEST DTOs
// ==========================================

public record InvoiceRequestDto(
    Guid Id,
    Guid ContractId,
    string? ContractNumber,
    Guid AccountId,
    string? AccountName,
    Guid? PaymentId,
    string? PaymentNumber,
    decimal Amount,
    string Status,
    Guid RequestedByUserId,
    string? RequestedByName,
    DateTime RequestDate,
    string? Notes,
    Guid? IssuedInvoiceId,
    string? IssuedInvoiceNumber,
    DateTime CreatedAt
);

public record CreateInvoiceRequestInput(
    Guid ContractId,
    Guid? AccountId,
    Guid? PaymentId,
    decimal Amount,
    string? Notes
);

public record InvoiceRequestFilterParams(
    int PageNumber = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? ContractId = null,
    Guid? AccountId = null
);

public record PagedInvoiceRequestResult(
    IReadOnlyList<InvoiceRequestDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

// ==========================================
// 6. RECEIVABLES & DEBT AGING DTOs
// ==========================================

public record AccountsReceivableItemDto(
    Guid AccountId,
    string AccountName,
    string? ContactPerson,
    string? ContactPhone,
    string? ContactEmail,
    Guid ContractId,
    string ContractNumber,
    Guid PaymentScheduleId,
    string MilestoneTitle,
    DateOnly DueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    int OverdueDays,
    string AgingBracket,
    DateTime? LastCollectionActivityDate,
    string? LastCollectionStatus,
    Guid? AssignedSalesEmployeeId,
    string? AssignedSalesEmployeeName
);

public record ReceivableAgingSummaryDto(
    decimal TotalReceivables,
    decimal TotalOverdue,
    decimal CurrentAmount,
    decimal Overdue1To7Days,
    decimal Overdue8To30Days,
    decimal Overdue31To60Days,
    decimal OverdueMoreThan60Days,
    int AccountsWithDebtCount,
    int OverdueContractsCount
);

public record ReceivableCollectionLogDto(
    Guid Id,
    Guid AccountId,
    string? AccountName,
    Guid ContractId,
    string? ContractNumber,
    Guid? PaymentScheduleId,
    string ActivityType,
    string Notes,
    Guid LoggedById,
    string LoggedByName,
    DateTime LoggedAt,
    DateOnly? NextFollowUpDate,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    string Status,
    DateTime CreatedAt
);

public record CreateReceivableCollectionLogRequest(
    Guid AccountId,
    Guid ContractId,
    Guid? PaymentScheduleId,
    string ActivityType,
    string Notes,
    DateOnly? NextFollowUpDate,
    string? Status
);

public record ReceivableFilterParams(
    string? SearchTerm = null,
    string? AgingBracket = null,
    Guid? AccountId = null,
    Guid? SalesEmployeeId = null,
    bool OnlyOverdue = false
);

// ==========================================
// 7. REVENUE MANAGEMENT DTOs
// ==========================================

public record RevenueFilterParams(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string PeriodType = "monthly",
    Guid? CustomerId = null,
    Guid? SalesEmployeeId = null,
    Guid? ContractId = null
);

public record RevenueSummaryDto(
    decimal TotalContractValue,
    decimal TotalInvoicedAmount,
    decimal TotalCollectedCash,
    decimal TotalOutstandingReceivables,
    decimal InvoicedPercentage,
    decimal CashCollectionRate
);

public record RevenuePeriodItemDto(
    string PeriodLabel,
    DateTime StartDate,
    DateTime EndDate,
    decimal ContractValue,
    decimal InvoicedAmount,
    decimal CollectedCash,
    decimal OutstandingAmount
);

public record RevenueByCustomerDto(
    Guid AccountId,
    string CustomerName,
    int ContractCount,
    decimal TotalContractValue,
    decimal InvoicedAmount,
    decimal CollectedCash,
    decimal OutstandingAmount
);

public record RevenueByContractDto(
    Guid ContractId,
    string ContractNumber,
    string CustomerName,
    decimal TotalValue,
    decimal InvoicedAmount,
    decimal CollectedCash,
    decimal OutstandingAmount,
    string Status
);

public record RevenueBySalesDto(
    Guid EmployeeId,
    string EmployeeName,
    int ContractCount,
    decimal TotalContractValue,
    decimal InvoicedAmount,
    decimal CollectedCash
);

public record RevenueByServiceDto(
    Guid? ServiceId,
    string ServiceName,
    int QuantitySold,
    decimal InvoicedAmount
);

public record RevenueAnalyticsResponse(
    RevenueSummaryDto Summary,
    IReadOnlyList<RevenuePeriodItemDto> TrendByPeriod,
    IReadOnlyList<RevenueByCustomerDto> ByCustomer,
    IReadOnlyList<RevenueByContractDto> ByContract,
    IReadOnlyList<RevenueBySalesDto> BySales,
    IReadOnlyList<RevenueByServiceDto> ByService
);

// ==========================================
// 8. FINANCIAL RECONCILIATION DTOs
// ==========================================

public record FinancialReconciliationRecordDto(
    Guid Id,
    string Period,
    DateOnly ReconciliationDate,
    decimal TotalContractValue,
    decimal TotalExpectedRevenue,
    decimal TotalCollectedRevenue,
    decimal TotalInvoicedRevenue,
    decimal DiscrepancyAmount,
    string Status,
    string? Notes,
    Guid ReconciledById,
    string ReconciledByName,
    Guid? ApprovedById,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    IReadOnlyList<ReconciliationItemDto>? Items = null
);

public record ReconciliationItemDto(
    Guid Id,
    Guid ReconciliationRecordId,
    Guid? ContractId,
    string? ContractNumber,
    Guid? PaymentId,
    string? PaymentNumber,
    Guid? InvoiceId,
    string? InvoiceNumber,
    string DiscrepancyType,
    decimal ExpectedAmount,
    decimal ActualAmount,
    decimal Difference,
    string ResolutionStatus,
    string? ResolutionNotes
);

public record RunReconciliationRequest(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string? Notes
);

public record ResolveDiscrepancyRequest(
    string Action,
    string ResolutionNotes
);
