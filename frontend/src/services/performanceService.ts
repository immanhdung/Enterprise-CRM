import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  KpiDefinition,
  CreateKpiDefinitionRequest,
  UpdateKpiDefinitionRequest,
  KpiAssignment,
  CreateKpiAssignmentRequest,
  EvaluateKpiRequest,
  ApproveKpiRequest,
  SalesPerformanceSummary,
  PerformanceRankingItem,
  PerformanceTimelineItem,
  CommissionPolicy,
  CreateCommissionPolicyRequest,
  CommissionCalculationItem,
  RunCommissionCalculationRequest,
  CommissionAdjustmentRequest,
  ApproveCommissionRequest,
  BonusPolicy,
  CreateBonusPolicyRequest,
  BonusRecord,
  CreateBonusRecordRequest,
  ApproveBonusRecordRequest,
  PayrollPeriod,
  CreatePayrollPeriodRequest,
  PayrollRecord,
  AddPayrollAdjustmentRequest,
  ApprovePayrollRequest,
  CompensationAuditLog,
} from '../types/performance';

export const performanceService = {
  // ==========================================
  // 1. KPI Management
  // ==========================================
  async getKpiDefinitions(): Promise<KpiDefinition[]> {
    const res = await apiClient.get<ApiResponse<KpiDefinition[]>>('/kpis/definitions');
    return res.data.data || [];
  },

  async getKpiDefinitionById(id: string): Promise<KpiDefinition> {
    const res = await apiClient.get<ApiResponse<KpiDefinition>>(`/kpis/definitions/${id}`);
    return res.data.data!;
  },

  async createKpiDefinition(req: CreateKpiDefinitionRequest): Promise<KpiDefinition> {
    const res = await apiClient.post<ApiResponse<KpiDefinition>>('/kpis/definitions', req);
    return res.data.data!;
  },

  async updateKpiDefinition(id: string, req: UpdateKpiDefinitionRequest): Promise<KpiDefinition> {
    const res = await apiClient.put<ApiResponse<KpiDefinition>>(`/kpis/definitions/${id}`, req);
    return res.data.data!;
  },

  async deleteKpiDefinition(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/kpis/definitions/${id}`);
    return res.data.data!;
  },

  async getKpiAssignments(params?: {
    month?: number;
    year?: number;
    employeeId?: string;
    departmentId?: string;
    status?: string;
  }): Promise<KpiAssignment[]> {
    const res = await apiClient.get<ApiResponse<KpiAssignment[]>>('/kpis/assignments', { params });
    return res.data.data || [];
  },

  async getKpiAssignmentById(id: string): Promise<KpiAssignment> {
    const res = await apiClient.get<ApiResponse<KpiAssignment>>(`/kpis/assignments/${id}`);
    return res.data.data!;
  },

  async createKpiAssignment(req: CreateKpiAssignmentRequest): Promise<KpiAssignment> {
    const res = await apiClient.post<ApiResponse<KpiAssignment>>('/kpis/assignments', req);
    return res.data.data!;
  },

  async evaluateKpiAssignment(id: string, req: EvaluateKpiRequest): Promise<KpiAssignment> {
    const res = await apiClient.post<ApiResponse<KpiAssignment>>(`/kpis/assignments/${id}/evaluate`, req);
    return res.data.data!;
  },

  async approveKpiAssignment(id: string, req: ApproveKpiRequest): Promise<KpiAssignment> {
    const res = await apiClient.post<ApiResponse<KpiAssignment>>(`/kpis/assignments/${id}/approve`, req);
    return res.data.data!;
  },

  async autoCalculateKpiActuals(id: string): Promise<KpiAssignment> {
    const res = await apiClient.post<ApiResponse<KpiAssignment>>(`/kpis/assignments/${id}/auto-calculate`);
    return res.data.data!;
  },

  // ==========================================
  // 2. Sales Performance Analytics
  // ==========================================
  async getPerformanceSummary(params?: {
    month?: number;
    year?: number;
    employeeId?: string;
    departmentId?: string;
    teamId?: string;
  }): Promise<SalesPerformanceSummary> {
    const res = await apiClient.get<ApiResponse<SalesPerformanceSummary>>('/performance/summary', { params });
    return res.data.data!;
  },

  async getEmployeeRankings(params?: {
    month?: number;
    year?: number;
    departmentId?: string;
  }): Promise<PerformanceRankingItem[]> {
    const res = await apiClient.get<ApiResponse<PerformanceRankingItem[]>>('/performance/rankings', { params });
    return res.data.data || [];
  },

  async getTimelineAnalytics(params?: {
    timeframe?: string;
    year?: number;
    employeeId?: string;
    departmentId?: string;
  }): Promise<PerformanceTimelineItem[]> {
    const res = await apiClient.get<ApiResponse<PerformanceTimelineItem[]>>('/performance/timeline', { params });
    return res.data.data || [];
  },

  // ==========================================
  // 3. Commission Policies & Calculations
  // ==========================================
  async getCommissionPolicies(params?: { activeOnly?: boolean }): Promise<CommissionPolicy[]> {
    const res = await apiClient.get<ApiResponse<CommissionPolicy[]>>('/commissions/policies', { params });
    return res.data.data || [];
  },

  async getCommissionPolicyById(id: string): Promise<CommissionPolicy> {
    const res = await apiClient.get<ApiResponse<CommissionPolicy>>(`/commissions/policies/${id}`);
    return res.data.data!;
  },

  async createCommissionPolicy(req: CreateCommissionPolicyRequest): Promise<CommissionPolicy> {
    const res = await apiClient.post<ApiResponse<CommissionPolicy>>('/commissions/policies', req);
    return res.data.data!;
  },

  async getCommissions(params?: {
    month?: number;
    year?: number;
    employeeId?: string;
    status?: string;
  }): Promise<CommissionCalculationItem[]> {
    const res = await apiClient.get<ApiResponse<CommissionCalculationItem[]>>('/commissions', { params });
    return res.data.data || [];
  },

  async getCommissionById(id: string): Promise<CommissionCalculationItem> {
    const res = await apiClient.get<ApiResponse<CommissionCalculationItem>>(`/commissions/${id}`);
    return res.data.data!;
  },

  async runCommissionCalculation(req: RunCommissionCalculationRequest): Promise<CommissionCalculationItem[]> {
    const res = await apiClient.post<ApiResponse<CommissionCalculationItem[]>>('/commissions/calculate', req);
    return res.data.data || [];
  },

  async approveCommission(id: string, req: ApproveCommissionRequest): Promise<CommissionCalculationItem> {
    const res = await apiClient.post<ApiResponse<CommissionCalculationItem>>(`/commissions/${id}/approve`, req);
    return res.data.data!;
  },

  async adjustCommission(
    idOrReq: string | CommissionAdjustmentRequest,
    maybeReq?: { adjustmentAmount: number; reason: string }
  ): Promise<CommissionCalculationItem> {
    const payload = typeof idOrReq === 'string'
      ? { commissionId: idOrReq, ...maybeReq }
      : idOrReq;
    const res = await apiClient.post<ApiResponse<CommissionCalculationItem>>('/commissions/adjust', payload);
    return res.data.data!;
  },

  async recalculateCommission(id: string): Promise<CommissionCalculationItem> {
    const res = await apiClient.post<ApiResponse<CommissionCalculationItem>>(`/commissions/${id}/recalculate`);
    return res.data.data!;
  },

  async getCompensationAuditLogs(params?: {
    entityType?: string;
    entityId?: string;
  }): Promise<CompensationAuditLog[]> {
    const res = await apiClient.get<ApiResponse<CompensationAuditLog[]>>('/commissions/audit-logs', { params });
    return res.data.data || [];
  },

  // ==========================================
  // 4. Bonus Management
  // ==========================================
  async getBonusPolicies(params?: { activeOnly?: boolean }): Promise<BonusPolicy[]> {
    const res = await apiClient.get<ApiResponse<BonusPolicy[]>>('/bonuses/policies', { params });
    return res.data.data || [];
  },

  async getBonusPolicyById(id: string): Promise<BonusPolicy> {
    const res = await apiClient.get<ApiResponse<BonusPolicy>>(`/bonuses/policies/${id}`);
    return res.data.data!;
  },

  async createBonusPolicy(req: CreateBonusPolicyRequest): Promise<BonusPolicy> {
    const res = await apiClient.post<ApiResponse<BonusPolicy>>('/bonuses/policies', req);
    return res.data.data!;
  },

  async getBonusRecords(params?: {
    month?: number;
    year?: number;
    employeeId?: string;
    status?: string;
  }): Promise<BonusRecord[]> {
    const res = await apiClient.get<ApiResponse<BonusRecord[]>>('/bonuses', { params });
    return res.data.data || [];
  },

  async getBonusRecordById(id: string): Promise<BonusRecord> {
    const res = await apiClient.get<ApiResponse<BonusRecord>>(`/bonuses/${id}`);
    return res.data.data!;
  },

  async createBonusRecord(req: CreateBonusRecordRequest): Promise<BonusRecord> {
    const res = await apiClient.post<ApiResponse<BonusRecord>>('/bonuses', req);
    return res.data.data!;
  },

  async approveBonusRecord(id: string, req: ApproveBonusRecordRequest): Promise<BonusRecord> {
    const res = await apiClient.post<ApiResponse<BonusRecord>>(`/bonuses/${id}/approve`, req);
    return res.data.data!;
  },

  // ==========================================
  // 5. Payroll Management
  // ==========================================
  async getPayrollPeriods(paramsOrYear?: number | { year?: number }): Promise<PayrollPeriod[]> {
    const params = typeof paramsOrYear === 'number' ? { year: paramsOrYear } : paramsOrYear;
    const res = await apiClient.get<ApiResponse<PayrollPeriod[]>>('/payroll/periods', { params });
    return res.data.data || [];
  },

  async getPayrollPeriodById(id: string): Promise<PayrollPeriod> {
    const res = await apiClient.get<ApiResponse<PayrollPeriod>>(`/payroll/periods/${id}`);
    return res.data.data!;
  },

  async createPayrollPeriod(req: CreatePayrollPeriodRequest): Promise<PayrollPeriod> {
    const res = await apiClient.post<ApiResponse<PayrollPeriod>>('/payroll/periods', req);
    return res.data.data!;
  },

  async processPayrollPeriod(periodId: string): Promise<PayrollPeriod> {
    const res = await apiClient.post<ApiResponse<PayrollPeriod>>(`/payroll/periods/${periodId}/process`);
    return res.data.data!;
  },

  async processPayroll(periodId: string): Promise<PayrollRecord[]> {
    await apiClient.post<ApiResponse<PayrollPeriod>>(`/payroll/periods/${periodId}/process`);
    return this.getPayrollRecords(periodId);
  },

  async approvePayrollPeriod(periodId: string, req: ApprovePayrollRequest): Promise<PayrollPeriod> {
    const res = await apiClient.post<ApiResponse<PayrollPeriod>>(`/payroll/periods/${periodId}/approve`, req);
    return res.data.data!;
  },

  async finalizePayrollPeriod(periodId: string): Promise<PayrollPeriod> {
    const res = await apiClient.post<ApiResponse<PayrollPeriod>>(`/payroll/periods/${periodId}/finalize`);
    return res.data.data!;
  },

  async getPayrollRecords(periodId: string, employeeId?: string): Promise<PayrollRecord[]> {
    const res = await apiClient.get<ApiResponse<PayrollRecord[]>>(`/payroll/periods/${periodId}/records`, {
      params: { employeeId },
    });
    return res.data.data || [];
  },

  async getPayrollRecordById(id: string): Promise<PayrollRecord> {
    const res = await apiClient.get<ApiResponse<PayrollRecord>>(`/payroll/records/${id}`);
    return res.data.data!;
  },

  async addPayrollAdjustment(
    recordIdOrReq: string | AddPayrollAdjustmentRequest,
    maybeReq?: AddPayrollAdjustmentRequest
  ): Promise<PayrollRecord> {
    const payload = typeof recordIdOrReq === 'string'
      ? { payrollRecordId: recordIdOrReq, ...maybeReq }
      : recordIdOrReq;
    const res = await apiClient.post<ApiResponse<PayrollRecord>>('/payroll/adjustments', payload);
    return res.data.data!;
  },

  async recalculatePayrollRecord(id: string): Promise<PayrollRecord> {
    const res = await apiClient.post<ApiResponse<PayrollRecord>>(`/payroll/records/${id}/recalculate`);
    return res.data.data!;
  },
};
