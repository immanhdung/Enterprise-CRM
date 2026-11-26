using System.Text.Json.Serialization;

namespace EnterpriseCRM.Application.Features.AutomationAndAi;

// ==================== WORKFLOW ENGINE DTOS ====================

public record WorkflowDefinitionDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    string TriggerEvent,
    string? ConditionsJson,
    string? ActionsJson,
    int ExecutionOrder,
    int Version,
    bool IsActive,
    int ExecutionCount,
    int ErrorCount,
    DateTime? LastExecutedAt,
    DateTime CreatedAt
);

public record CreateWorkflowDefinitionRequest(
    string Name,
    string Description,
    string Category,
    string TriggerEvent,
    string? ConditionsJson,
    string? ActionsJson,
    int ExecutionOrder = 1,
    bool IsActive = true
);

public record UpdateWorkflowDefinitionRequest(
    string Name,
    string Description,
    string Category,
    string TriggerEvent,
    string? ConditionsJson,
    string? ActionsJson,
    int ExecutionOrder,
    bool IsActive
);

public record WorkflowExecutionDto(
    Guid Id,
    Guid WorkflowId,
    string WorkflowName,
    string TriggerEvent,
    string EntityType,
    Guid EntityId,
    string Status,
    DateTime ExecutedAt,
    long DurationMs,
    string? ResultSummary,
    string? ErrorMessage,
    List<WorkflowExecutionLogDto> Logs
);

public record WorkflowExecutionLogDto(
    Guid Id,
    Guid WorkflowExecutionId,
    string StepName,
    string ActionType,
    string Status,
    string? InputJson,
    string? OutputJson,
    string? ErrorMessage,
    DateTime ExecutedAt
);

public record ExecuteWorkflowRequest(
    string TriggerEvent,
    string EntityType,
    Guid EntityId,
    Dictionary<string, object>? ContextData = null
);

// ==================== LEAD ASSIGNMENT DTOS ====================

public record LeadAssignmentRuleDto(
    Guid Id,
    string Name,
    string Description,
    int Priority,
    string Mode,
    string? ConditionsJson,
    string? TargetEmployeeIdsJson,
    string? TargetTeamIdsJson,
    int? MaxActiveLeadsPerRep,
    bool IsActive
);

public record CreateLeadAssignmentRuleRequest(
    string Name,
    string Description,
    int Priority,
    string Mode,
    string? ConditionsJson,
    string? TargetEmployeeIdsJson,
    string? TargetTeamIdsJson,
    int? MaxActiveLeadsPerRep,
    bool IsActive = true
);

public record LeadAssignmentLogDto(
    Guid Id,
    Guid LeadId,
    string LeadName,
    Guid? RuleId,
    string? RuleName,
    Guid? PreviousEmployeeId,
    string? PreviousEmployeeName,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    string Mode,
    string Reason,
    Guid? AssignedByUserId,
    DateTime Timestamp
);

public record AssignLeadRequest(
    Guid LeadId,
    Guid? SpecificEmployeeId = null,
    string? Mode = null,
    string? Reason = null
);

public record RepWorkloadDto(
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    string? TeamName,
    int ActiveLeadsCount,
    int MaxCapacity,
    decimal CapacityUtilizationPercent,
    bool IsAvailable
);

public record LeadDistributionMetricsDto(
    int TotalAssignedToday,
    int TotalAssignedThisMonth,
    int TotalUnassignedLeads,
    Dictionary<string, int> DistributionByMode,
    List<RepWorkloadDto> RepWorkloads
);

// ==================== SMART ALERTS DTOS ====================

public record AlertRuleDto(
    Guid Id,
    string Name,
    string Category,
    string AlertType,
    int ThresholdDays,
    decimal? ThresholdAmount,
    string Severity,
    string Channels,
    bool IsActive,
    string? Description
);

public record CreateAlertRuleRequest(
    string Name,
    string Category,
    string AlertType,
    int ThresholdDays,
    decimal? ThresholdAmount,
    string Severity,
    string Channels,
    bool IsActive = true,
    string? Description = null
);

public record BusinessAlertDto(
    Guid Id,
    Guid? AlertRuleId,
    string Title,
    string Message,
    string Category,
    string EntityType,
    Guid EntityId,
    string Severity,
    string Status,
    Guid? AssignedToUserId,
    DateTime TriggeredAt,
    DateTime? AcknowledgedAt,
    Guid? AcknowledgedBy,
    DateTime? ResolvedAt,
    Guid? ResolvedBy,
    string? ResolutionNotes
);

public record AcknowledgeAlertRequest(
    string? ResolutionNotes
);

public record NotificationPreferenceDto(
    Guid Id,
    Guid UserId,
    string Category,
    bool InAppEnabled,
    bool EmailEnabled,
    string MinSeverity
);

public record UpdateNotificationPreferenceRequest(
    string Category,
    bool InAppEnabled,
    bool EmailEnabled,
    string MinSeverity
);

// ==================== AI ASSISTANT & QUERY DTOS ====================

public record AiConversationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string? ContextSnapshotJson,
    DateTime CreatedAt,
    int MessageCount
);

public record AiConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    string? ContextJson,
    int TokensUsed,
    string? ModelUsed,
    DateTime CreatedAtUtc
);

public record AiChatRequest(
    Guid? ConversationId,
    string Message,
    string? CurrentPageContext = null
);

public record AiChatResponse(
    Guid ConversationId,
    AiConversationMessageDto Message,
    bool IsFallbackDiagnostic
);

public record BusinessQueryRequest(
    string Query,
    string? Period = null
);

public record BusinessQueryResultDto(
    string Query,
    string MetricName,
    string AnswerText,
    string Period,
    List<string> Columns,
    List<Dictionary<string, object>> Rows,
    string? ChartType,
    string? ChartDataJson,
    string? SummaryNote,
    bool Success
);

public record AiProviderConfigDto(
    Guid Id,
    string ProviderType,
    string? EndpointUrl,
    string ModelName,
    decimal Temperature,
    int MaxTokens,
    bool IsActive,
    bool HasApiKey
);

public record UpdateAiProviderConfigRequest(
    string ProviderType,
    string? EndpointUrl,
    string ModelName,
    string? ApiKey,
    decimal Temperature,
    int MaxTokens,
    bool IsActive
);

public record AiUsageLogDto(
    Guid Id,
    Guid UserId,
    string OperationType,
    string ProviderType,
    string ModelName,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    long DurationMs,
    bool Success,
    string? ErrorMessage,
    DateTime Timestamp
);

public record AiUsageSummaryDto(
    int TotalRequests,
    int TotalTokens,
    decimal TotalEstimatedCostUsd,
    Dictionary<string, int> RequestsByOperation,
    List<AiUsageLogDto> RecentLogs
);

// ==================== AI ANALYTICS & FORECASTING DTOS ====================

public record SalesAnalysisSummaryDto(
    string OverviewText,
    List<string> Strengths,
    List<string> Weaknesses,
    List<string> ActionableRecommendations,
    decimal CurrentMonthWinRate,
    decimal PipelineHealthScore
);

public record ForecastDataPointDto(
    string DateLabel,
    decimal Amount,
    bool IsForecast
);

public record RevenueForecastDto(
    Guid Id,
    DateTime ForecastDate,
    int HorizonDays,
    string PeriodLabel,
    decimal PredictedAmount,
    decimal LowerBound,
    decimal UpperBound,
    decimal ConfidenceScore,
    string? AssumptionsJson,
    string ModelType,
    List<ForecastDataPointDto> HistoricalTrend,
    List<ForecastDataPointDto> ForecastPoints
);

public record GenerateForecastRequest(
    int HorizonDays = 30
);

public record AnomalyDetectionDto(
    Guid Id,
    string MetricName,
    string Category,
    string Severity,
    decimal ActualValue,
    decimal ExpectedValue,
    decimal DeviationPercent,
    string Description,
    DateTime DetectedAt,
    string Status,
    string? ResolutionNotes
);

public record ResolveAnomalyRequest(
    string Status,
    string? ResolutionNotes
);

public record BusinessRecommendationDto(
    Guid Id,
    string Category,
    string TargetEntityType,
    Guid TargetEntityId,
    string TargetEntityName,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTime GeneratedAt,
    string? ActionPayloadJson
);

public record ApplyRecommendationRequest(
    string Action // Apply, Dismiss
);

// ==================== MANAGEMENT REPORTING DTOS ====================

public record ScheduledReportDto(
    Guid Id,
    string Name,
    string ReportType,
    List<string> RecipientsEmails,
    string Format,
    string TimeOfDay,
    int? DayOfWeek,
    int? DayOfMonth,
    bool IsActive,
    DateTime? LastRunAt
);

public record CreateScheduledReportRequest(
    string Name,
    string ReportType,
    List<string> RecipientsEmails,
    string Format,
    string TimeOfDay,
    int? DayOfWeek,
    int? DayOfMonth,
    bool IsActive = true
);

public record GeneratedReportDto(
    Guid Id,
    Guid? ScheduledReportId,
    string ReportType,
    string Title,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string SummaryJson,
    string FullDataJson,
    string? FileUrl,
    DateTime GeneratedAt
);

public record GenerateReportRequest(
    string ReportType, // Daily, Weekly, Monthly
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null
);

public record RepPerformanceReportItemDto(
    string EmployeeName,
    string TeamName,
    decimal AchievedRevenue,
    int WonDeals,
    decimal KpiAchievementRate
);

public record ManagementReportDataDto(
    string Title,
    string PeriodLabel,
    decimal TotalRevenue,
    decimal TotalCollectedCash,
    decimal TotalReceivables,
    int NewLeadsCount,
    int QualifiedLeadsCount,
    int DealsCreatedCount,
    int DealsWonCount,
    decimal DealsWonValue,
    int ActiveContractsCount,
    decimal ActiveContractsValue,
    decimal TotalPayrollPaid,
    List<RepPerformanceReportItemDto> TopPerformers
);

// ==================== ROLE-BASED DASHBOARD DTOS ====================

public record SalesDashboardDto(
    Guid EmployeeId,
    string EmployeeName,
    int AssignedLeadsCount,
    int ActiveDealsCount,
    decimal PipelineValue,
    decimal TargetRevenue,
    decimal AchievedRevenue,
    decimal RevenueAttainmentPercent,
    int WonDealsCount,
    int PendingTasksCount,
    int TodayAppointmentsCount,
    decimal EstimatedCommission,
    List<DealSummaryItemDto> KeyDeals,
    List<TaskSummaryItemDto> UrgentTasks
);

public record DealSummaryItemDto(
    Guid Id,
    string Title,
    string AccountName,
    decimal Amount,
    string StageName,
    int Probability,
    DateTime? ExpectedCloseDate
);

public record TaskSummaryItemDto(
    Guid Id,
    string Title,
    string Priority,
    DateTime? DueDate,
    string Status
);

public record ManagerDashboardDto(
    Guid ManagerId,
    string DepartmentName,
    int TeamMembersCount,
    decimal TeamTargetRevenue,
    decimal TeamAchievedRevenue,
    decimal AttainmentPercent,
    decimal TotalPipelineValue,
    int TotalWonDealsCount,
    int UnassignedLeadsCount,
    int StagnantDealsCount,
    List<RepWorkloadDto> TeamWorkloads,
    List<RepPerformanceReportItemDto> Leaderboard
);

public record ExecutiveDashboardDto(
    decimal TotalRevenue,
    decimal TotalCollectedCash,
    decimal TotalReceivables,
    decimal OverdueDebtAmount,
    decimal TotalPipelineValue,
    int TotalDealsInPipeline,
    int TotalWonDealsYearToDate,
    decimal WinRatePercent,
    decimal GrossMarginPercent,
    decimal CurrentMonthPayrollTotal,
    Dictionary<string, decimal> ReceivablesAging,
    List<DealSummaryItemDto> TopDealsClosingSoon,
    List<AnomalyDetectionDto> ActiveCriticalAnomalies,
    List<BusinessRecommendationDto> HighPriorityRecommendations
);
