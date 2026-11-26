namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

// ==================== KPI DTOs ====================

public record KpiDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string MetricType,
    string Unit,
    decimal DefaultTarget,
    string? Description,
    bool IsActive
);

public record CreateKpiDefinitionRequest(
    string Code,
    string Name,
    string Category,
    string MetricType,
    string Unit,
    decimal DefaultTarget,
    string? Description
);

public record UpdateKpiDefinitionRequest(
    string Name,
    string Category,
    string MetricType,
    string Unit,
    decimal DefaultTarget,
    string? Description,
    bool IsActive
);

public record KpiAssignmentDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    Guid? DefinitionId,
    string? DefinitionCode,
    string? DefinitionName,
    Guid? TeamId,
    string? TeamName,
    Guid? DepartmentId,
    string? DepartmentName,
    string Scope,
    int PeriodMonth,
    int PeriodYear,
    int? PeriodQuarter,
    decimal TargetRevenue,
    decimal AchievedRevenue,
    int TargetDealsWon,
    int AchievedDealsWon,
    int TargetNewLeads,
    int AchievedNewLeads,
    int TargetCalls,
    int AchievedCalls,
    int TargetMeetings,
    int AchievedMeetings,
    int TargetTasksCompleted,
    int AchievedTasksCompleted,
    decimal TargetConversionRate,
    decimal AchievedConversionRate,
    decimal WeightPercent,
    decimal AchievementScore,
    string Status,
    string? ApprovalNotes,
    Guid? EvaluatedBy,
    DateTime? EvaluatedAt,
    Guid? ApprovedBy,
    DateTime? ApprovedAt
);

public record CreateKpiAssignmentRequest(
    Guid EmployeeId,
    Guid? DefinitionId,
    Guid? TeamId,
    Guid? DepartmentId,
    string Scope,
    int PeriodMonth,
    int PeriodYear,
    int? PeriodQuarter,
    decimal TargetRevenue,
    int TargetDealsWon,
    int TargetNewLeads,
    int TargetCalls,
    int TargetMeetings,
    int TargetTasksCompleted,
    decimal TargetConversionRate,
    decimal WeightPercent,
    string? Notes
);

public record UpdateKpiAssignmentRequest(
    decimal TargetRevenue,
    int TargetDealsWon,
    int TargetNewLeads,
    int TargetCalls,
    int TargetMeetings,
    int TargetTasksCompleted,
    decimal TargetConversionRate,
    decimal WeightPercent,
    string? Notes
);

public record EvaluateKpiRequest(
    decimal? AchievedRevenue,
    int? AchievedDealsWon,
    int? AchievedNewLeads,
    int? AchievedCalls,
    int? AchievedMeetings,
    int? AchievedTasksCompleted,
    decimal? AchievedConversionRate,
    string? EvaluationNotes
);

public record ApproveKpiRequest(
    bool Approved,
    string? Comments
);

// ==================== SALES PERFORMANCE ANALYTICS DTOs ====================

public record SalesPerformanceSummaryDto(
    int TotalAssignedLeads,
    int QualifiedLeads,
    int ConvertedLeads,
    decimal LeadConversionRate,
    int TotalDeals,
    int WonDeals,
    int LostDeals,
    decimal DealWinRate,
    decimal AverageDealValue,
    int AverageSalesCycleDays,
    decimal ContractValue,
    decimal InvoicedAmount,
    decimal CollectedCashRevenue,
    decimal OutstandingReceivables,
    decimal NewCustomerRevenue,
    decimal ExistingCustomerRevenue,
    // Month-over-month / Period comparison
    decimal RevenueGrowthPercent,
    decimal DealsGrowthPercent,
    decimal ConversionGrowthPercent
);

public record PerformanceRankingItemDto(
    int Rank,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    string? DepartmentName,
    string? TeamName,
    int WonDeals,
    decimal ContractValue,
    decimal CollectedCash,
    decimal KpiAchievementPercent
);

public record PerformanceTimelineItemDto(
    string PeriodLabel,
    decimal ContractValue,
    decimal InvoicedAmount,
    decimal CollectedCash,
    int WonDeals
);

// ==================== COMMISSION POLICY DTOs ====================

public record CommissionTierRule(
    decimal MinAchievement,
    decimal? MaxAchievement,
    decimal Rate,
    decimal? FixedBonus
);

public record CommissionPolicyDto(
    Guid Id,
    string Name,
    string PolicyType,
    string CalculationBasis,
    string CustomerType,
    Guid? ServiceId,
    string? ServiceName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? TeamId,
    string? TeamName,
    decimal MinThreshold,
    decimal? MaxThreshold,
    decimal? MaxCap,
    decimal RatePercent,
    int Version,
    string? Description,
    string? TierRulesJson,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive
);

public record CreateCommissionPolicyRequest(
    string Name,
    string PolicyType,
    string CalculationBasis,
    string CustomerType,
    Guid? ServiceId,
    Guid? DepartmentId,
    Guid? TeamId,
    decimal MinThreshold,
    decimal? MaxThreshold,
    decimal? MaxCap,
    decimal RatePercent,
    string? Description,
    string? TierRulesJson,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo
);

public record UpdateCommissionPolicyRequest(
    string Name,
    string PolicyType,
    string CalculationBasis,
    string CustomerType,
    Guid? ServiceId,
    Guid? DepartmentId,
    Guid? TeamId,
    decimal MinThreshold,
    decimal? MaxThreshold,
    decimal? MaxCap,
    decimal RatePercent,
    string? Description,
    string? TierRulesJson,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive
);

public record CommissionCalculationItemDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    Guid? PolicyId,
    string? PolicyName,
    string CalculationBasis,
    int PeriodMonth,
    int PeriodYear,
    Guid? DealId,
    string? DealTitle,
    Guid? ContractId,
    string? ContractNumber,
    Guid? PaymentId,
    string? TransactionReference,
    decimal BaseAmount,
    decimal RateApplied,
    decimal CalculatedAmount,
    decimal AdjustmentAmount,
    decimal NetCommission,
    string Status,
    string? Notes,
    int PolicyVersion,
    string? SourceType,
    Guid? ApprovedBy,
    DateTime? ApprovedAt
);

public record RunCommissionCalculationRequest(
    int PeriodMonth,
    int PeriodYear,
    Guid? EmployeeId,
    Guid? DepartmentId,
    Guid? PolicyId
);

public record CommissionAdjustmentRequest(
    Guid CommissionId,
    decimal AdjustmentAmount,
    string Reason
);

public record ApproveCommissionRequest(
    bool Approved,
    string? Notes
);

// ==================== BONUS DTOs ====================

public record BonusPolicyDto(
    Guid Id,
    string Name,
    string Type,
    string Period,
    decimal TargetThreshold,
    decimal BonusAmount,
    string? Description,
    bool IsActive
);

public record CreateBonusPolicyRequest(
    string Name,
    string Type,
    string Period,
    decimal TargetThreshold,
    decimal BonusAmount,
    string? Description
);

public record UpdateBonusPolicyRequest(
    string Name,
    string Type,
    string Period,
    decimal TargetThreshold,
    decimal BonusAmount,
    string? Description,
    bool IsActive
);

public record BonusRecordDto(
    Guid Id,
    Guid? BonusPolicyId,
    string? BonusPolicyName,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    int PeriodMonth,
    int PeriodYear,
    decimal Amount,
    string Reason,
    string Status,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    Guid? PayrollRecordId,
    string? Notes
);

public record CreateBonusRecordRequest(
    Guid? BonusPolicyId,
    Guid EmployeeId,
    int PeriodMonth,
    int PeriodYear,
    decimal Amount,
    string Reason,
    string? Notes
);

public record ApproveBonusRecordRequest(
    bool Approved,
    string? Notes
);

// ==================== PAYROLL DTOs ====================

public record PayrollPeriodDto(
    Guid Id,
    string Name,
    int Month,
    int Year,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    decimal TotalBaseSalary,
    decimal TotalCommission,
    decimal TotalBonuses,
    decimal TotalAllowances,
    decimal TotalDeductions,
    decimal TotalGross,
    decimal TotalNet,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime? FinalizedAt,
    string? ApprovalNotes,
    int EmployeeCount
);

public record CreatePayrollPeriodRequest(
    string Name,
    int Month,
    int Year,
    DateOnly StartDate,
    DateOnly EndDate
);

public record PayrollRecordDto(
    Guid Id,
    Guid PayrollPeriodId,
    string PeriodName,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    string? DepartmentName,
    string? Position,
    decimal BaseSalary,
    decimal TotalCommission,
    decimal Bonuses,
    decimal Allowances,
    decimal Deductions,
    decimal GrossSalary,
    decimal NetSalary,
    decimal CommissionEarned,
    decimal CommissionApproved,
    decimal CommissionPayable,
    decimal CommissionPaid,
    string Status,
    DateOnly? PaymentDate,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    string? Notes,
    List<PayrollAdjustmentDto> Adjustments
);

public record PayrollAdjustmentDto(
    Guid Id,
    Guid PayrollRecordId,
    string Type,
    decimal Amount,
    string Reason,
    Guid? ApprovedBy,
    DateTime? ApprovedAt
);

public record AddPayrollAdjustmentRequest(
    Guid PayrollRecordId,
    string Type,
    decimal Amount,
    string Reason
);

public record ApprovePayrollRequest(
    string RoleLevel, // Manager, Director/CEO
    bool Approved,
    string? Comments
);

public record CompensationAuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid ActorUserId,
    string? OldValueJson,
    string? NewValueJson,
    string Reason,
    DateTime Timestamp
);
