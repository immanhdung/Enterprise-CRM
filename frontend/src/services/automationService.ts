import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  WorkflowDefinition,
  CreateWorkflowRequest,
  WorkflowExecution,
  LeadAssignmentRule,
  CreateLeadAssignmentRuleRequest,
  LeadAssignmentLog,
  LeadDistributionMetrics,
  AlertRule,
  BusinessAlert,
  NotificationPreference,
} from '../types/automation';

export const automationService = {
  // ==========================================
  // 1. Workflow Automation
  // ==========================================
  async getWorkflows(category?: string, isActive?: boolean): Promise<WorkflowDefinition[]> {
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    if (isActive !== undefined) params.append('isActive', String(isActive));

    const res = await apiClient.get<ApiResponse<WorkflowDefinition[]>>(`/workflows?${params.toString()}`);
    return res.data.data || [];
  },

  async getWorkflowById(id: string): Promise<WorkflowDefinition> {
    const res = await apiClient.get<ApiResponse<WorkflowDefinition>>(`/workflows/${id}`);
    return res.data.data!;
  },

  async createWorkflow(request: CreateWorkflowRequest): Promise<WorkflowDefinition> {
    const res = await apiClient.post<ApiResponse<WorkflowDefinition>>('/workflows', request);
    return res.data.data!;
  },

  async updateWorkflow(id: string, request: CreateWorkflowRequest): Promise<WorkflowDefinition> {
    const res = await apiClient.put<ApiResponse<WorkflowDefinition>>(`/workflows/${id}`, request);
    return res.data.data!;
  },

  async deleteWorkflow(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/workflows/${id}`);
    return res.data.data || false;
  },

  async triggerWorkflow(triggerEvent: string, entityType: string, entityId: string, contextData?: Record<string, any>): Promise<WorkflowExecution> {
    const res = await apiClient.post<ApiResponse<WorkflowExecution>>('/workflows/trigger', {
      triggerEvent,
      entityType,
      entityId,
      contextData,
    });
    return res.data.data!;
  },

  async getExecutions(workflowId?: string, limit = 50): Promise<WorkflowExecution[]> {
    const params = new URLSearchParams();
    if (workflowId) params.append('workflowId', workflowId);
    params.append('limit', String(limit));

    const res = await apiClient.get<ApiResponse<WorkflowExecution[]>>(`/workflows/executions?${params.toString()}`);
    return res.data.data || [];
  },

  async getExecutionById(id: string): Promise<WorkflowExecution> {
    const res = await apiClient.get<ApiResponse<WorkflowExecution>>(`/workflows/executions/${id}`);
    return res.data.data!;
  },

  // ==========================================
  // 2. Automatic Lead Distribution
  // ==========================================
  async getLeadRules(): Promise<LeadAssignmentRule[]> {
    const res = await apiClient.get<ApiResponse<LeadAssignmentRule[]>>('/lead-assignment/rules');
    return res.data.data || [];
  },

  async createLeadRule(request: CreateLeadAssignmentRuleRequest): Promise<LeadAssignmentRule> {
    const res = await apiClient.post<ApiResponse<LeadAssignmentRule>>('/lead-assignment/rules', request);
    return res.data.data!;
  },

  async updateLeadRule(id: string, request: CreateLeadAssignmentRuleRequest): Promise<LeadAssignmentRule> {
    const res = await apiClient.put<ApiResponse<LeadAssignmentRule>>(`/lead-assignment/rules/${id}`, request);
    return res.data.data!;
  },

  async deleteLeadRule(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/lead-assignment/rules/${id}`);
    return res.data.data || false;
  },

  async assignLead(leadId: string, specificEmployeeId?: string, mode?: string, reason?: string): Promise<LeadAssignmentLog> {
    const res = await apiClient.post<ApiResponse<LeadAssignmentLog>>('/lead-assignment/assign', {
      leadId,
      specificEmployeeId,
      mode,
      reason,
    });
    return res.data.data!;
  },

  async getAssignmentLogs(leadId?: string, limit = 50): Promise<LeadAssignmentLog[]> {
    const params = new URLSearchParams();
    if (leadId) params.append('leadId', leadId);
    params.append('limit', String(limit));

    const res = await apiClient.get<ApiResponse<LeadAssignmentLog[]>>(`/lead-assignment/logs?${params.toString()}`);
    return res.data.data || [];
  },

  async getLeadDistributionMetrics(): Promise<LeadDistributionMetrics> {
    const res = await apiClient.get<ApiResponse<LeadDistributionMetrics>>('/lead-assignment/metrics');
    return res.data.data!;
  },

  // ==========================================
  // 3. Smart Alerts & Reminders
  // ==========================================
  async getActiveAlerts(category?: string, severity?: string): Promise<BusinessAlert[]> {
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    if (severity) params.append('severity', severity);

    const res = await apiClient.get<ApiResponse<BusinessAlert[]>>(`/alerts?${params.toString()}`);
    return res.data.data || [];
  },

  async acknowledgeAlert(id: string, notes?: string): Promise<BusinessAlert> {
    const res = await apiClient.post<ApiResponse<BusinessAlert>>(`/alerts/${id}/acknowledge`, { resolutionNotes: notes });
    return res.data.data!;
  },

  async resolveAlert(id: string, notes?: string): Promise<BusinessAlert> {
    const res = await apiClient.post<ApiResponse<BusinessAlert>>(`/alerts/${id}/resolve`, { resolutionNotes: notes });
    return res.data.data!;
  },

  async runAlertScan(): Promise<number> {
    const res = await apiClient.post<ApiResponse<number>>('/alerts/scan');
    return res.data.data || 0;
  },

  async getAlertRules(): Promise<AlertRule[]> {
    const res = await apiClient.get<ApiResponse<AlertRule[]>>('/alerts/rules');
    return res.data.data || [];
  },

  async createAlertRule(request: Partial<AlertRule>): Promise<AlertRule> {
    const res = await apiClient.post<ApiResponse<AlertRule>>('/alerts/rules', request);
    return res.data.data!;
  },

  async updateAlertRule(id: string, request: Partial<AlertRule>): Promise<AlertRule> {
    const res = await apiClient.put<ApiResponse<AlertRule>>(`/alerts/rules/${id}`, request);
    return res.data.data!;
  },

  async deleteAlertRule(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/alerts/rules/${id}`);
    return res.data.data || false;
  },

  async getNotificationPreferences(): Promise<NotificationPreference> {
    const res = await apiClient.get<ApiResponse<NotificationPreference>>('/alerts/preferences');
    return res.data.data!;
  },

  async updateNotificationPreferences(request: Partial<NotificationPreference>): Promise<NotificationPreference> {
    const res = await apiClient.put<ApiResponse<NotificationPreference>>('/alerts/preferences', request);
    return res.data.data!;
  },
};
