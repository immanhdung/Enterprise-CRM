import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  AiConversation,
  AiConversationMessage,
  AiChatRequest,
  AiChatResponse,
  BusinessQueryRequest,
  BusinessQueryResult,
  AiProviderConfig,
  UpdateAiProviderConfigRequest,
  AiUsageSummary,
  SalesAnalysisSummary,
  RevenueForecast,
  AnomalyDetection,
  ResolveAnomalyRequest,
  BusinessRecommendation,
  ScheduledReport,
  GeneratedReport,
  ManagementReportData,
  SalesDashboard,
  ManagerDashboard,
  ExecutiveDashboard,
} from '../types/ai';

export const aiService = {
  // ==========================================
  // 1. AI Assistant & Conversations
  // ==========================================
  async getConversations(): Promise<AiConversation[]> {
    const res = await apiClient.get<ApiResponse<AiConversation[]>>('/ai-assistant/conversations');
    return res.data.data || [];
  },

  async getConversationById(id: string): Promise<AiConversation> {
    const res = await apiClient.get<ApiResponse<AiConversation>>(`/ai-assistant/conversations/${id}`);
    return res.data.data!;
  },

  async getConversationMessages(conversationId: string): Promise<AiConversationMessage[]> {
    const res = await apiClient.get<ApiResponse<AiConversationMessage[]>>(`/ai-assistant/conversations/${conversationId}/messages`);
    return res.data.data || [];
  },

  async sendChatMessage(request: AiChatRequest): Promise<AiChatResponse> {
    const res = await apiClient.post<ApiResponse<AiChatResponse>>('/ai-assistant/chat', request);
    return res.data.data!;
  },

  async deleteConversation(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/ai-assistant/conversations/${id}`);
    return res.data.data || false;
  },

  async executeBusinessQuery(request: BusinessQueryRequest): Promise<BusinessQueryResult> {
    const res = await apiClient.post<ApiResponse<BusinessQueryResult>>('/ai-assistant/query', request);
    return res.data.data!;
  },

  async getProviderConfig(): Promise<AiProviderConfig> {
    const res = await apiClient.get<ApiResponse<AiProviderConfig>>('/ai-assistant/config');
    return res.data.data!;
  },

  async updateProviderConfig(request: UpdateAiProviderConfigRequest): Promise<AiProviderConfig> {
    const res = await apiClient.put<ApiResponse<AiProviderConfig>>('/ai-assistant/config', request);
    return res.data.data!;
  },

  async getUsageSummary(): Promise<AiUsageSummary> {
    const res = await apiClient.get<ApiResponse<AiUsageSummary>>('/ai-assistant/usage');
    return res.data.data!;
  },

  // ==========================================
  // 2. AI Business Analytics & Forecast
  // ==========================================
  async getPerformanceAnalysis(month?: number, year?: number): Promise<SalesAnalysisSummary> {
    const params = new URLSearchParams();
    if (month) params.append('month', String(month));
    if (year) params.append('year', String(year));

    const res = await apiClient.get<ApiResponse<SalesAnalysisSummary>>(`/ai-analytics/overview?${params.toString()}`);
    return res.data.data!;
  },

  async getRevenueForecast(horizonDays = 30): Promise<RevenueForecast> {
    const res = await apiClient.get<ApiResponse<RevenueForecast>>(`/ai-analytics/forecast?horizonDays=${horizonDays}`);
    return res.data.data!;
  },

  async recalculateForecast(horizonDays = 30): Promise<RevenueForecast> {
    const res = await apiClient.post<ApiResponse<RevenueForecast>>('/ai-analytics/forecast/recalculate', { horizonDays });
    return res.data.data!;
  },

  async getAnomalies(category?: string, status?: string): Promise<AnomalyDetection[]> {
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    if (status) params.append('status', status);

    const res = await apiClient.get<ApiResponse<AnomalyDetection[]>>(`/ai-analytics/anomalies?${params.toString()}`);
    return res.data.data || [];
  },

  async runAnomalyScan(): Promise<number> {
    const res = await apiClient.post<ApiResponse<number>>('/ai-analytics/anomalies/scan');
    return res.data.data || 0;
  },

  async resolveAnomaly(id: string, request: ResolveAnomalyRequest): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/ai-analytics/anomalies/${id}/resolve`, request);
    return res.data.data || false;
  },

  async getRecommendations(status?: string): Promise<BusinessRecommendation[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    const res = await apiClient.get<ApiResponse<BusinessRecommendation[]>>(`/ai-analytics/recommendations?${params.toString()}`);
    return res.data.data || [];
  },

  async applyRecommendation(id: string, action: 'Apply' | 'Dismiss'): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/ai-analytics/recommendations/${id}/apply`, { action });
    return res.data.data || false;
  },

  // ==========================================
  // 3. Automated Management Reporting
  // ==========================================
  async getScheduledReports(): Promise<ScheduledReport[]> {
    const res = await apiClient.get<ApiResponse<ScheduledReport[]>>('/reports/scheduled');
    return res.data.data || [];
  },

  async createScheduledReport(request: Partial<ScheduledReport>): Promise<ScheduledReport> {
    const res = await apiClient.post<ApiResponse<ScheduledReport>>('/reports/scheduled', request);
    return res.data.data!;
  },

  async deleteScheduledReport(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/reports/scheduled/${id}`);
    return res.data.data || false;
  },

  async getGeneratedReports(reportType?: string, limit = 30): Promise<GeneratedReport[]> {
    const params = new URLSearchParams();
    if (reportType) params.append('reportType', reportType);
    params.append('limit', String(limit));

    const res = await apiClient.get<ApiResponse<GeneratedReport[]>>(`/reports/generated?${params.toString()}`);
    return res.data.data || [];
  },

  async generateReport(reportType: 'Daily' | 'Weekly' | 'Monthly', periodStart?: string, periodEnd?: string): Promise<GeneratedReport> {
    const res = await apiClient.post<ApiResponse<GeneratedReport>>('/reports/generate', {
      reportType,
      periodStart,
      periodEnd,
    });
    return res.data.data!;
  },

  async getReportData(reportType: string, periodStart: string, periodEnd: string): Promise<ManagementReportData> {
    const params = new URLSearchParams({ reportType, periodStart, periodEnd });
    const res = await apiClient.get<ApiResponse<ManagementReportData>>(`/reports/data?${params.toString()}`);
    return res.data.data!;
  },

  // ==========================================
  // 4. Role-Based Dashboards
  // ==========================================
  async getSalesDashboard(employeeId?: string): Promise<SalesDashboard> {
    const params = new URLSearchParams();
    if (employeeId) params.append('employeeId', employeeId);
    const res = await apiClient.get<ApiResponse<SalesDashboard>>(`/dashboards/sales?${params.toString()}`);
    return res.data.data!;
  },

  async getManagerDashboard(managerId?: string): Promise<ManagerDashboard> {
    const params = new URLSearchParams();
    if (managerId) params.append('managerId', managerId);
    const res = await apiClient.get<ApiResponse<ManagerDashboard>>(`/dashboards/manager?${params.toString()}`);
    return res.data.data!;
  },

  async getExecutiveDashboard(): Promise<ExecutiveDashboard> {
    const res = await apiClient.get<ApiResponse<ExecutiveDashboard>>('/dashboards/executive');
    return res.data.data!;
  },
};
