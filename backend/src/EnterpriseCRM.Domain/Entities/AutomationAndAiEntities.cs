using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

// ==================== WORKFLOW EXECUTION LOG ====================

public class WorkflowExecutionLog : BaseEntity
{
    public Guid WorkflowExecutionId { get; set; }
    public WorkflowExecution WorkflowExecution { get; set; } = null!;

    public string StepName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty; // AssignLead, CreateTask, CreateAppointment, SendNotification, SendEmail, UpdateStatus
    public string Status { get; set; } = "Success"; // Success, Failed, Skipped
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

// ==================== LEAD ASSIGNMENT ENGINE ====================

public class LeadAssignmentRule : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string Mode { get; set; } = "RoundRobin"; // RoundRobin, Workload, Territory, Source, ServiceSpecialization
    public string? ConditionsJson { get; set; } // Conditions e.g. source, territory, budget
    public string? TargetEmployeeIdsJson { get; set; } // Eligible employee IDs
    public string? TargetTeamIdsJson { get; set; } // Eligible team IDs
    public int? MaxActiveLeadsPerRep { get; set; } // Workload limit cap
    public bool IsActive { get; set; } = true;
}

public class LeadAssignmentLog : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;

    public Guid? RuleId { get; set; }
    public LeadAssignmentRule? Rule { get; set; }

    public Guid? PreviousEmployeeId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    public string Mode { get; set; } = "Manual";
    public string Reason { get; set; } = string.Empty;
    public Guid? AssignedByUserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// ==================== SMART ALERTS & REMINDERS ====================

public class AlertRule : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Lead"; // Lead, Deal, Customer, Finance
    public string AlertType { get; set; } = "StagnantLead"; // StagnantLead, NoFollowUp, OverdueActivity, StagnantDeal, ClosingSoon, ExpiringContract, OverduePayment, OverdueReceivable, HighValueAtRisk
    public int ThresholdDays { get; set; } = 3;
    public decimal? ThresholdAmount { get; set; }
    public string Severity { get; set; } = "Warning"; // Info, Warning, Critical
    public string Channels { get; set; } = "InApp,Email"; // InApp, Email
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}

public class BusinessAlert : BaseEntity
{
    public Guid? AlertRuleId { get; set; }
    public AlertRule? AlertRule { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Lead, Deal, Customer, Finance
    public string EntityType { get; set; } = string.Empty; // Lead, Deal, Contract, Payment, Invoice
    public Guid EntityId { get; set; }
    public string Severity { get; set; } = "Warning"; // Info, Warning, Critical
    public string Status { get; set; } = "Active"; // Active, Acknowledged, Resolved, Dismissed

    public Guid? AssignedToUserId { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Category { get; set; } = "All"; // Lead, Deal, Customer, Finance, System
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public string MinSeverity { get; set; } = "Info"; // Info, Warning, Critical
}

// ==================== AI ASSISTANT & CONVERSATIONS ====================

public class AiConversation : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = "New Business Conversation";
    public string? ContextSnapshotJson { get; set; }
    public ICollection<AiConversationMessage> Messages { get; set; } = new List<AiConversationMessage>();
}

public class AiConversationMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public AiConversation Conversation { get; set; } = null!;

    public string Role { get; set; } = "user"; // user, assistant, system
    public string Content { get; set; } = string.Empty;
    public string? ContextJson { get; set; } // Tabular data or chart config returned by query
    public int TokensUsed { get; set; } = 0;
    public string? ModelUsed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class AiProviderConfiguration : BaseEntity, IAuditableEntity
{
    public string ProviderType { get; set; } = "Gemini"; // Gemini, OpenAI, AzureOpenAI, LocalCompatible
    public string? EndpointUrl { get; set; }
    public string ModelName { get; set; } = "gemini-1.5-flash"; // or gpt-4o-mini
    public string? ApiKeyEncrypted { get; set; }
    public decimal Temperature { get; set; } = 0.2m;
    public int MaxTokens { get; set; } = 2048;
    public bool IsActive { get; set; } = true;
}

public class AiUsageLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string OperationType { get; set; } = "Chat"; // Chat, NaturalLanguageQuery, AnalyticsSummary, Forecast, AnomalyExplanation
    public string ProviderType { get; set; } = "Gemini";
    public string ModelName { get; set; } = string.Empty;
    public int PromptTokens { get; set; } = 0;
    public int CompletionTokens { get; set; } = 0;
    public int TotalTokens { get; set; } = 0;
    public long DurationMs { get; set; } = 0;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// ==================== AI ANALYTICS, FORECASTING & ANOMALIES ====================

public class RevenueForecast : BaseEntity, IAuditableEntity
{
    public DateTime ForecastDate { get; set; } = DateTime.UtcNow;
    public int HorizonDays { get; set; } = 30; // 7, 30, 90, 365
    public string PeriodLabel { get; set; } = string.Empty; // Next 7 Days, Next 30 Days, Q1 2027
    public decimal PredictedAmount { get; set; } = 0;
    public decimal LowerBound { get; set; } = 0;
    public decimal UpperBound { get; set; } = 0;
    public decimal ConfidenceScore { get; set; } = 0.85m; // 85% confidence
    public string? AssumptionsJson { get; set; }
    public string ModelType { get; set; } = "WeightedPipelineRegression";
}

public class AnomalyDetection : BaseEntity
{
    public string MetricName { get; set; } = string.Empty; // RevenueDrop, DealLossSpike, StagnantDealCluster, PaymentDelaySpike, KpiDeviation
    public string Category { get; set; } = "Revenue"; // Revenue, Pipeline, Lead, Finance, Performance
    public string Severity { get; set; } = "Warning"; // Low, Medium, High, Critical
    public decimal ActualValue { get; set; } = 0;
    public decimal ExpectedValue { get; set; } = 0;
    public decimal DeviationPercent { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active, Investigating, Resolved, FalsePositive
    public Guid? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class BusinessRecommendation : BaseEntity
{
    public string Category { get; set; } = "LeadPriority"; // LeadPriority, DealIntervention, CustomerRenewal, UpsellOpportunity, TeamImprovement
    public string TargetEntityType { get; set; } = "Lead"; // Lead, Deal, Account, Contract, Team
    public Guid TargetEntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent
    public string Status { get; set; } = "Pending"; // Pending, Applied, Dismissed
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? ActionPayloadJson { get; set; }
}

// ==================== MANAGEMENT REPORTING ====================

public class ScheduledReport : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = "Monthly"; // Daily, Weekly, Monthly, AdHoc
    public string RecipientsEmailJson { get; set; } = "[]";
    public string Format { get; set; } = "PDF"; // PDF, Excel, InApp
    public string TimeOfDay { get; set; } = "08:00"; // 08:00 UTC+7
    public int? DayOfWeek { get; set; } // For weekly reports (e.g. 1 = Monday)
    public int? DayOfMonth { get; set; } // For monthly reports (e.g. 1 = 1st of month)
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
}

public class GeneratedReport : BaseEntity
{
    public Guid? ScheduledReportId { get; set; }
    public ScheduledReport? ScheduledReport { get; set; }

    public string ReportType { get; set; } = "Monthly"; // Daily, Weekly, Monthly
    public string Title { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string SummaryJson { get; set; } = "{}";
    public string FullDataJson { get; set; } = "{}";
    public string? FileUrl { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
