using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class KpiDefinition : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Sales"; // Sales, Activity, Conversion
    public string MetricType { get; set; } = "Currency"; // Currency, Count, Percentage
    public string Unit { get; set; } = "VND"; // VND, Items, %, Calls, Meetings
    public decimal DefaultTarget { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Kpi : BaseEntity, IAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid? DefinitionId { get; set; }
    public KpiDefinition? Definition { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string Scope { get; set; } = "Individual"; // Individual, Team, Department

    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public int? PeriodQuarter { get; set; }

    public decimal TargetRevenue { get; set; } = 0;
    public decimal AchievedRevenue { get; set; } = 0;

    public int TargetDealsWon { get; set; } = 0;
    public int AchievedDealsWon { get; set; } = 0;

    public int TargetNewLeads { get; set; } = 0;
    public int AchievedNewLeads { get; set; } = 0;

    public int TargetCalls { get; set; } = 0;
    public int AchievedCalls { get; set; } = 0;

    public int TargetMeetings { get; set; } = 0;
    public int AchievedMeetings { get; set; } = 0;

    public int TargetTasksCompleted { get; set; } = 0;
    public int AchievedTasksCompleted { get; set; } = 0;

    public decimal TargetConversionRate { get; set; } = 0; // percentage e.g. 20.0%
    public decimal AchievedConversionRate { get; set; } = 0;

    public decimal WeightPercent { get; set; } = 100; // e.g. 50%
    public decimal AchievementScore { get; set; } = 0; // calculated overall achievement percentage

    public string Status { get; set; } = "Draft"; // Draft, PendingApproval, Approved, Evaluated, Rejected
    public string? ApprovalNotes { get; set; }

    public Guid? EvaluatedBy { get; set; }
    public DateTime? EvaluatedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class CommissionPolicy : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string PolicyType { get; set; } = "RevenueBased"; // RevenueBased, CollectedCashBased, ServiceTierBased
    public string CalculationBasis { get; set; } = "ContractValue"; // ContractValue, CollectedCash, ServiceBased, CustomerTypeBased, TargetTierBased
    public string CustomerType { get; set; } = "All"; // All, NewCustomer, ExistingCustomer

    public Guid? ServiceId { get; set; }
    public ServiceItem? Service { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public decimal MinThreshold { get; set; } = 0;
    public decimal? MaxThreshold { get; set; }
    public decimal? MaxCap { get; set; }
    public decimal RatePercent { get; set; } // e.g. 5.0 for 5%
    public int Version { get; set; } = 1;
    public string? Description { get; set; }
    public string? TierRulesJson { get; set; } // JSON array of tier ranges

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

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public Guid? PolicyId { get; set; }
    public CommissionPolicy? Policy { get; set; }

    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }

    public string CalculationBasis { get; set; } = "ContractValue"; // ContractValue, CollectedCash, ServiceBased, CustomerTypeBased, TargetTierBased
    public decimal BaseAmount { get; set; } = 0;
    public decimal RateApplied { get; set; } = 0;
    public decimal CalculatedAmount { get; set; } = 0;
    public decimal AdjustmentAmount { get; set; } = 0;
    public decimal NetCommission { get; set; } = 0;

    public string Status { get; set; } = "Calculated"; // Calculated, Approved, Paid, Cancelled, Rejected
    public Guid? PayrollRecordId { get; set; }
    public string? Notes { get; set; }
    public int PolicyVersion { get; set; } = 1;
    public string? SourceType { get; set; } = "Contract"; // Contract, Deal, Payment, ManualAdjustment

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class BonusPolicy : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "IndividualPerformance"; // IndividualPerformance, TeamPerformance, DepartmentPerformance, NewCustomerAcquisition, RevenueTarget, SpecialAchievement
    public string Period { get; set; } = "Monthly"; // Monthly, Quarterly, Annual, AdHoc
    public decimal TargetThreshold { get; set; } = 0;
    public decimal BonusAmount { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BonusRecord : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? BonusPolicyId { get; set; }
    public BonusPolicy? BonusPolicy { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public decimal Amount { get; set; } = 0;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, PendingApproval, Approved, Rejected, Paid

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Guid? PayrollRecordId { get; set; }
    public PayrollRecord? PayrollRecord { get; set; }

    public string? Notes { get; set; }
}

public class PayrollPeriod : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Processing, Locked, Finalized, Paid

    public decimal TotalBaseSalary { get; set; } = 0;
    public decimal TotalCommission { get; set; } = 0;
    public decimal TotalBonuses { get; set; } = 0;
    public decimal TotalAllowances { get; set; } = 0;
    public decimal TotalDeductions { get; set; } = 0;
    public decimal TotalGross { get; set; } = 0;
    public decimal TotalNet { get; set; } = 0;

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public string? ApprovalNotes { get; set; }

    public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
}

public class PayrollRecord : BaseEntity, IAuditableEntity
{
    public Guid PayrollPeriodId { get; set; }
    public PayrollPeriod PayrollPeriod { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public decimal BaseSalary { get; set; } = 0;
    public decimal TotalCommission { get; set; } = 0;
    public decimal Bonuses { get; set; } = 0;
    public decimal Allowances { get; set; } = 0;
    public decimal Deductions { get; set; } = 0;
    public decimal GrossSalary { get; set; } = 0;
    public decimal NetSalary { get; set; } = 0;

    public decimal CommissionEarned { get; set; } = 0;
    public decimal CommissionApproved { get; set; } = 0;
    public decimal CommissionPayable { get; set; } = 0;
    public decimal CommissionPaid { get; set; } = 0;

    public string Status { get; set; } = "PendingReview"; // Draft, PendingReview, ApprovedByManager, ApprovedByDirector, Finalized, Paid
    public DateOnly? PaymentDate { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<PayrollAdjustment> Adjustments { get; set; } = new List<PayrollAdjustment>();
}

public class PayrollAdjustment : BaseEntity
{
    public Guid PayrollRecordId { get; set; }
    public PayrollRecord PayrollRecord { get; set; } = null!;

    public string Type { get; set; } = "Bonus"; // Bonus, Penalty, Advance, TaxAdjustment, Allowance
    public decimal Amount { get; set; } = 0;
    public string Reason { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class CompensationAuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Commission, Bonus, PayrollRecord, PayrollPeriod, KPI
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Calculate, Approve, Reject, Adjust, Finalize, Recalculate
    public Guid ActorUserId { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
