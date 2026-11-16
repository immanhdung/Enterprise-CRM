using EnterpriseCRM.Domain.Common;
using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Domain.Entities;

public class ServiceCategory : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ServiceItem> Services { get; set; } = new List<ServiceItem>();
}

public class PriceList : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}

public class PriceListItem : BaseEntity
{
    public Guid PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public ServiceItem Service { get; set; } = null!;

    public decimal UnitPrice { get; set; }
    public decimal MinQuantity { get; set; } = 1;
    public decimal MaxDiscountRate { get; set; } = 0; // max allowed discount %
}

public class ServicePriceHistory : BaseEntity
{
    public Guid ServiceId { get; set; }
    public ServiceItem Service { get; set; } = null!;

    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public Guid? ChangedById { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}

public class ServiceItem : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public ServiceCategory? ServiceCategory { get; set; }
    public string Category { get; set; } = "Service";
    public string Unit { get; set; } = "Project"; // Project, Hour, Month, License, Seat
    public decimal BasePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal VatRate { get; set; } = 8.0m;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PriceListItem> PriceListItems { get; set; } = new List<PriceListItem>();
    public ICollection<ServicePriceHistory> PriceHistories { get; set; } = new List<ServicePriceHistory>();
    public ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
    public ICollection<ContractItem> ContractItems { get; set; } = new List<ContractItem>();
}

public class Quotation : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string QuotationNumber { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string RevisionNumber { get; set; } = "REV-01";
    public Guid? ParentQuotationId { get; set; }
    public Quotation? ParentQuotation { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? PriceListId { get; set; }
    public PriceList? PriceList { get; set; }

    public DateOnly ValidUntil { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected, Sent, Expired, Accepted

    public string? TermsAndConditions { get; set; }
    public string? Notes { get; set; }
    public string? PdfUrl { get; set; }

    // Approval tracking
    public DateTime? SubmittedAt { get; set; }
    public Guid? SubmittedById { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Customer dispatch tracking
    public DateTime? SentAt { get; set; }
    public string? SentToEmail { get; set; }
    public Guid? SentById { get; set; }

    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
    public ICollection<QuotationApprovalHistory> ApprovalHistories { get; set; } = new List<QuotationApprovalHistory>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Quotation> Revisions { get; set; } = new List<Quotation>();
}

public class QuotationItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public ServiceItem Service { get; set; } = null!;

    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Unit";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; } // % discount
    public decimal DiscountAmount { get; set; }
    public decimal VatRate { get; set; } = 8.0m;
    public decimal VatAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class QuotationApprovalHistory : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public string Action { get; set; } = string.Empty; // Submitted, Approved, Rejected, Revised, Sent
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ContractTemplate : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // MSA, SOW, SLA, NDA
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty; // Markdown or HTML with {{Placeholders}}
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}

public class Contract : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? TemplateId { get; set; }
    public ContractTemplate? Template { get; set; }

    public Guid? ParentContractId { get; set; }
    public Contract? ParentContract { get; set; }
    public int RenewalCount { get; set; } = 0;

    public string Title { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalValue { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public string EsignStatus { get; set; } = "NotSent"; // NotSent, PendingSignature, Signed, Declined, Expired
    public string? DocumentUrl { get; set; }
    public string? SignedDocumentUrl { get; set; }

    // Terms
    public string? ScopeOfWork { get; set; }
    public string? PaymentTerms { get; set; }
    public string? SpecialTerms { get; set; }

    // E-signature metadata
    public DateTime? SignedAt { get; set; }
    public string? SignedByClientName { get; set; }
    public string? SignedByClientEmail { get; set; }
    public string? SignerIpAddress { get; set; }

    public ICollection<ContractItem> Items { get; set; } = new List<ContractItem>();
    public ICollection<ContractAppendix> Appendices { get; set; } = new List<ContractAppendix>();
    public ICollection<ContractHistory> Histories { get; set; } = new List<ContractHistory>();
    public ICollection<SignatureRequest> SignatureRequests { get; set; } = new List<SignatureRequest>();
    public ICollection<Contract> Renewals { get; set; } = new List<Contract>();
    public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class ContractItem : BaseEntity
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public ServiceItem Service { get; set; } = null!;

    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Unit";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class ContractAppendix : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public string AppendixNumber { get; set; } = string.Empty; // e.g. APP-01
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal AdjustedValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? DocumentUrl { get; set; }
}

public class ContractHistory : BaseEntity
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public string Action { get; set; } = string.Empty; // Created, Updated, GeneratedFromQuotation, SentForSignature, Signed, Renewed, Terminated
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SignatureRequest : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public string Provider { get; set; } = "Mock"; // Mock, DocuSign, VnptEContract, AdobeSign
    public string ExternalEnvelopeId { get; set; } = string.Empty;
    public string SignerName { get; set; } = string.Empty;
    public string SignerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent"; // Sent, Delivered, Signed, Declined, Expired
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? SignedAt { get; set; }
    public string? SigningUrl { get; set; }
    public string? WebhookLog { get; set; }
}

public class Project : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string Name { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly Deadline { get; set; }
    public string Status { get; set; } = "Planning"; // Planning, InProgress, OnHold, Completed, Cancelled
    public int ProgressPercent { get; set; } = 0;
}

public class PaymentSchedule : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public int InstallmentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; } = 0;
    public string Status { get; set; } = "Pending"; // Pending, PartiallyPaid, Paid, Overdue

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class Payment : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? PaymentScheduleId { get; set; }
    public PaymentSchedule? PaymentSchedule { get; set; }

    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    public bool IsVerified { get; set; } = true;
    public Guid? VerifiedBy { get; set; }
}

public class InvoiceRequest : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public Guid RequestedBy { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Rejected
}

public class Invoice : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? InvoiceRequestId { get; set; }
    public InvoiceRequest? InvoiceRequest { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public string EInvoiceProvider { get; set; } = "Mock"; // MISA, VNPT, Viettel, Mock
    public string? ProviderTransactionId { get; set; }
    public DateOnly IssueDate { get; set; }
    public decimal TaxRate { get; set; } = 8.0m;
    public decimal TaxAmount { get; set; }
    public decimal TotalWithTax { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? PdfUrl { get; set; }
    public string? XmlUrl { get; set; }
}
