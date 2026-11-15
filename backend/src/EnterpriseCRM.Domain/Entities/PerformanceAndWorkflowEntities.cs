using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class Kpi : BaseEntity, IAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public decimal TargetRevenue { get; set; }
    public int TargetDealsWon { get; set; }
    public int TargetNewLeads { get; set; }
    public decimal AchievedRevenue { get; set; } = 0;
    public int AchievedDealsWon { get; set; } = 0;
}

public class CommissionPolicy : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string PolicyType { get; set; } = "RevenueBased"; // RevenueBased, CollectedCashBased, ServiceTierBased
    public decimal MinThreshold { get; set; } = 0;
    public decimal? MaxThreshold { get; set; }
    public decimal RatePercent { get; set; } // e.g. 5.0 for 5%
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Commission : BaseEntity, IAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public Guid? PolicyId { get; set; }
    public CommissionPolicy? Policy { get; set; }

    public decimal CalculatedAmount { get; set; }
    public string Status { get; set; } = "Calculated"; // Calculated, Approved, Paid, Cancelled
    public Guid? PayrollRecordId { get; set; }
}

public class PayrollPeriod : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Processing, Locked, Paid

    public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
}

public class PayrollRecord : BaseEntity, IAuditableEntity
{
    public Guid PayrollPeriodId { get; set; }
    public PayrollPeriod PayrollPeriod { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public decimal BaseSalary { get; set; }
    public decimal TotalCommission { get; set; } = 0;
    public decimal Bonuses { get; set; } = 0;
    public decimal Deductions { get; set; } = 0;
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = "PendingReview"; // PendingReview, ApprovedByManager, ApprovedByDirector, Disbursed

    public ICollection<PayrollAdjustment> Adjustments { get; set; } = new List<PayrollAdjustment>();
}

public class PayrollAdjustment : BaseEntity
{
    public Guid PayrollRecordId { get; set; }
    public PayrollRecord PayrollRecord { get; set; } = null!;

    public string Type { get; set; } = "Bonus"; // Bonus, Penalty, Advance, TaxAdjustment
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
}

public class WorkflowDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TriggerEvent { get; set; } = string.Empty; // LeadCreated, DealWon, PaymentOverdue
    public string? ConditionsJson { get; set; }
    public string? ActionsJson { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WorkflowExecution : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public WorkflowDefinition Workflow { get; set; } = null!;

    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Status { get; set; } = "Success"; // Success, Failed
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string? ResultSummary { get; set; }
}

public class Notification : BaseEntity
{
    public Guid RecipientUserId { get; set; }
    public User RecipientUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; } = false;
    public string Type { get; set; } = "Info"; // Info, Warning, Success, Alert
}
