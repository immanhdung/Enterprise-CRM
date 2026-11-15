using EnterpriseCRM.Domain.Common;
using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Domain.Entities;

public class ServiceItem : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Service";
    public string Unit { get; set; } = "Project";
    public decimal BasePrice { get; set; }
    public decimal VatRate { get; set; } = 8.0m;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Quotation : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string QuotationNumber { get; set; } = string.Empty;
    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public DateOnly ValidUntil { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected, Sent
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}

public class QuotationItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public ServiceItem Service { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal LineTotal { get; set; }
}

public class Contract : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalValue { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public string EsignStatus { get; set; } = "NotSent";
    public string? DocumentUrl { get; set; }
    public string? SignedDocumentUrl { get; set; }

    public ICollection<ContractItem> Items { get; set; } = new List<ContractItem>();
    public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
}

public class ContractItem : BaseEntity
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public ServiceItem Service { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
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
