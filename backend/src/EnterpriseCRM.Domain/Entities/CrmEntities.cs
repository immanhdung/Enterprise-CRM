using EnterpriseCRM.Domain.Common;
using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Domain.Entities;

public class LeadSource : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // FacebookAds, GoogleAds, TikTokAds, WebsiteForm, ManualEntry, Other
    public string? UtmSource { get; set; }
    public string? UtmCampaign { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}

public class Lead : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? SourceId { get; set; }
    public LeadSource? Source { get; set; }

    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? CompanyName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? JobTitle { get; set; }
    public decimal EstimatedBudget { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public int Score { get; set; } = 0;
    public bool IsKeyAccount { get; set; } = false;

    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent
    public string? Campaign { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    // Converted linkage
    public Guid? ConvertedAccountId { get; set; }
    public Account? ConvertedAccount { get; set; }

    public Guid? ConvertedContactId { get; set; }
    public Contact? ConvertedContact { get; set; }

    public Guid? ConvertedDealId { get; set; }
    public Deal? ConvertedDeal { get; set; }

    public DateTime? ConvertedAt { get; set; }

    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
}

public class LeadActivity : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;

    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string ActivityType { get; set; } = "Note"; // Call, Email, Meeting, Note, StatusChange, Assignment
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class Account : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? BillingAddress { get; set; }
    public string Classification { get; set; } = "Standard"; // VIP, Standard, Prospect, Partner
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
    public string? Description { get; set; }

    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class Contact : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Department { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
}

public class PipelineStage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public decimal Probability { get; set; }
    public bool IsWonStage { get; set; } = false;
    public bool IsLostStage { get; set; } = false;
    public string ColorCode { get; set; } = "#3B82F6";

    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
}

public class Deal : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid StageId { get; set; }
    public PipelineStage Stage { get; set; } = null!;

    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    public decimal Amount { get; set; }
    public decimal? ProbabilityOverride { get; set; }
    public decimal EffectiveProbability => ProbabilityOverride ?? Stage?.Probability ?? 0;
    public decimal ExpectedRevenue => Amount * (EffectiveProbability / 100m);

    public DateOnly ExpectedClosingDate { get; set; }
    public DateOnly? ActualClosingDate { get; set; }
    public DealStatus Status { get; set; } = DealStatus.Open;
    public string? LostReason { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }

    public ICollection<DealStageHistory> StageHistories { get; set; } = new List<DealStageHistory>();
}

public class DealStageHistory : BaseEntity
{
    public Guid DealId { get; set; }
    public Deal Deal { get; set; } = null!;

    public Guid? FromStageId { get; set; }
    public PipelineStage? FromStage { get; set; }

    public Guid ToStageId { get; set; }
    public PipelineStage ToStage { get; set; } = null!;

    public Guid? ChangedByUserId { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public int DurationDays { get; set; } = 0;
    public string? Notes { get; set; }
}

public class TaskItem : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Overdue, Cancelled
}

public class Appointment : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, Rescheduled
}

public class CrmActivity : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public string ActivityType { get; set; } = "Note"; // Call, Email, Meeting, Note, StatusChange, StageChange, Assignment, TaskCompleted, DealConverted
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByName { get; set; }

    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
