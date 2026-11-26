// ==================== KPI TYPES ====================

export interface KpiDefinition {
  id: string;
  code: string;
  name: string;
  category: 'Sales' | 'Activity' | 'Conversion';
  metricType: 'Currency' | 'Count' | 'Percentage';
  unit: string;
  defaultTarget: number;
  description?: string;
  isActive: boolean;
}

export interface CreateKpiDefinitionRequest {
  code: string;
  name: string;
  category: string;
  metricType: string;
  unit: string;
  defaultTarget: number;
  description?: string;
}

export interface UpdateKpiDefinitionRequest {
  name: string;
  category: string;
  metricType: string;
  unit: string;
  defaultTarget: number;
  description?: string;
  isActive: boolean;
}

export interface KpiAssignment {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  definitionId?: string;
  definitionCode?: string;
  definitionName?: string;
  teamId?: string;
  teamName?: string;
  departmentId?: string;
  departmentName?: string;
  scope: string;
  periodMonth: number;
  periodYear: number;
  periodQuarter?: number;
  targetRevenue: number;
  achievedRevenue: number;
  targetDealsWon: number;
  achievedDealsWon: number;
  targetNewLeads: number;
  achievedNewLeads: number;
  targetCalls: number;
  achievedCalls: number;
  targetMeetings: number;
  achievedMeetings: number;
  targetTasksCompleted: number;
  achievedTasksCompleted: number;
  targetConversionRate: number;
  achievedConversionRate: number;
  weightPercent: number;
  achievementScore: number;
  status: 'Draft' | 'PendingApproval' | 'Approved' | 'Evaluated' | 'Rejected';
  approvalNotes?: string;
  evaluatedBy?: string;
  evaluatedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface CreateKpiAssignmentRequest {
  employeeId: string;
  definitionId?: string;
  teamId?: string;
  departmentId?: string;
  scope: string;
  periodMonth: number;
  periodYear: number;
  periodQuarter?: number;
  targetRevenue: number;
  targetDealsWon: number;
  targetNewLeads: number;
  targetCalls: number;
  targetMeetings: number;
  targetTasksCompleted: number;
  targetConversionRate: number;
  weightPercent: number;
  notes?: string;
}

export interface EvaluateKpiRequest {
  achievedRevenue?: number;
  achievedDealsWon?: number;
  achievedNewLeads?: number;
  achievedCalls?: number;
  achievedMeetings?: number;
  achievedTasksCompleted?: number;
  achievedConversionRate?: number;
  evaluationNotes?: string;
}

export interface ApproveKpiRequest {
  approved: boolean;
  comments?: string;
}

// ==================== SALES PERFORMANCE ANALYTICS ====================

export interface SalesPerformanceSummary {
  totalAssignedLeads: number;
  qualifiedLeads: number;
  convertedLeads: number;
  leadConversionRate: number;
  totalDeals: number;
  wonDeals: number;
  lostDeals: number;
  dealWinRate: number;
  averageDealValue: number;
  averageSalesCycleDays: number;
  contractValue: number;
  invoicedAmount: number;
  collectedCashRevenue: number;
  outstandingReceivables: number;
  newCustomerRevenue: number;
  existingCustomerRevenue: number;
  revenueGrowthPercent: number;
  dealsGrowthPercent: number;
  conversionGrowthPercent: number;
  // Aliases for convenience
  totalContractValue?: number;
  totalCollectedCash?: number;
  totalDealsWon?: number;
  winRate?: number;
}

export interface PerformanceRankingItem {
  rank: number;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  departmentName?: string;
  teamName?: string;
  wonDeals: number;
  contractValue: number;
  collectedCash: number;
  kpiAchievementPercent: number;
  // Aliases for display
  dealsWon?: number;
  contractRevenue?: number;
  kpiScore?: number;
  winRate?: number;
}

export interface PerformanceTimelineItem {
  periodLabel: string;
  contractValue: number;
  invoicedAmount: number;
  collectedCash: number;
  wonDeals: number;
}

// ==================== COMMISSION TYPES ====================

export interface CommissionTierRule {
  minAchievement: number;
  maxAchievement?: number;
  rate: number;
  fixedBonus?: number;
}

export interface CommissionPolicy {
  id: string;
  name: string;
  code?: string;
  policyType?: string;
  calculationBasis: string;
  customerType: string;
  serviceId?: string;
  serviceName?: string;
  departmentId?: string;
  departmentName?: string;
  teamId?: string;
  teamName?: string;
  minThreshold: number;
  maxThreshold?: number;
  maxCap?: number;
  ratePercent: number;
  rate?: number;
  version: number;
  description?: string;
  tierRulesJson?: string;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
}

export interface CreateCommissionPolicyRequest {
  name: string;
  code?: string;
  policyType?: string;
  calculationBasis: string;
  customerType?: string;
  serviceId?: string;
  departmentId?: string;
  teamId?: string;
  minThreshold?: number;
  maxThreshold?: number;
  maxCap?: number;
  ratePercent?: number;
  rate?: number;
  description?: string;
  tierRulesJson?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
}

export interface CommissionCalculationItem {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  policyId?: string;
  policyName?: string;
  calculationBasis: string;
  periodMonth: number;
  periodYear: number;
  dealId?: string;
  dealTitle?: string;
  contractId?: string;
  contractNumber?: string;
  paymentId?: string;
  transactionReference?: string;
  baseAmount: number;
  rateApplied: number;
  rate?: number;
  amount?: number;
  calculatedAmount: number;
  adjustmentAmount: number;
  netCommission: number;
  status: 'Calculated' | 'Approved' | 'Paid' | 'Cancelled' | 'Rejected';
  notes?: string;
  policyVersion: number;
  sourceType?: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface RunCommissionCalculationRequest {
  periodMonth?: number;
  periodYear?: number;
  month?: number;
  year?: number;
  employeeId?: string;
  departmentId?: string;
  policyId?: string;
}

export interface CommissionAdjustmentRequest {
  commissionId?: string;
  adjustmentAmount: number;
  reason: string;
}

export interface ApproveCommissionRequest {
  approved?: boolean;
  status?: string;
  notes?: string;
}

// ==================== BONUS TYPES ====================

export interface BonusPolicy {
  id: string;
  name: string;
  type: string;
  period: string;
  targetThreshold: number;
  bonusAmount: number;
  description?: string;
  isActive: boolean;
}

export interface CreateBonusPolicyRequest {
  name: string;
  type: string;
  period: string;
  targetThreshold: number;
  bonusAmount: number;
  description?: string;
}

export interface BonusRecord {
  id: string;
  bonusPolicyId?: string;
  bonusPolicyName?: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  periodMonth: number;
  periodYear: number;
  amount: number;
  reason: string;
  status: 'Draft' | 'PendingApproval' | 'Approved' | 'Rejected' | 'Paid';
  approvedBy?: string;
  approvedAt?: string;
  payrollRecordId?: string;
  notes?: string;
}

export interface CreateBonusRecordRequest {
  bonusPolicyId?: string;
  employeeId: string;
  periodMonth: number;
  periodYear: number;
  amount: number;
  reason: string;
  notes?: string;
}

export interface ApproveBonusRecordRequest {
  approved: boolean;
  status?: string;
  notes?: string;
}

// ==================== PAYROLL TYPES ====================

export interface PayrollPeriod {
  id: string;
  name: string;
  month?: number;
  year?: number;
  startDate: string;
  endDate: string;
  status: 'Draft' | 'Processing' | 'Locked' | 'ApprovedByManager' | 'ApprovedByDirector' | 'PendingManagerApproval' | 'PendingDirectorApproval' | 'Approved' | 'Finalized' | 'Paid';
  totalBaseSalary: number;
  totalCommission: number;
  totalBonuses: number;
  totalAllowances: number;
  totalDeductions: number;
  totalGross: number;
  totalNet: number;
  approvedBy?: string;
  approvedAt?: string;
  finalizedAt?: string;
  approvalNotes?: string;
  employeeCount?: number;
}

export interface CreatePayrollPeriodRequest {
  name: string;
  month?: number;
  year?: number;
  startDate: string;
  endDate: string;
}

export interface PayrollAdjustment {
  id: string;
  payrollRecordId: string;
  type: 'Bonus' | 'Penalty' | 'Advance' | 'TaxAdjustment' | 'Allowance';
  amount: number;
  reason: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface PayrollRecord {
  id: string;
  payrollPeriodId: string;
  periodName?: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  departmentName?: string;
  position?: string;
  baseSalary: number;
  totalCommission?: number;
  bonuses: number;
  allowances: number;
  deductions: number;
  grossSalary: number;
  netSalary: number;
  commissionEarned?: number;
  commissionApproved?: number;
  commissionPayable?: number;
  commissionPaid?: number;
  status?: 'Draft' | 'PendingReview' | 'ApprovedByManager' | 'ApprovedByDirector' | 'Finalized' | 'Paid';
  paymentDate?: string;
  approvedBy?: string;
  approvedAt?: string;
  notes?: string;
  adjustments?: PayrollAdjustment[];
}

export interface AddPayrollAdjustmentRequest {
  payrollRecordId?: string;
  type?: string;
  amount?: number;
  reason: string;
  allowanceDelta?: number;
  deductionDelta?: number;
}

export interface ApprovePayrollRequest {
  roleLevel?: 'Manager' | 'Director' | 'CEO';
  approvalLevel?: 'Manager' | 'Director' | 'CEO';
  approved?: boolean;
  isApproved?: boolean;
  comments?: string;
  notes?: string;
}

// ==================== COMPENSATION AUDIT LOG ====================

export interface CompensationAuditLog {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  actorUserId?: string;
  actorName?: string;
  oldValueJson?: string;
  newValueJson?: string;
  reason: string;
  timestamp: string;
}
