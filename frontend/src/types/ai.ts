export interface AiConversation {
  id: string;
  userId: string;
  title: string;
  contextSnapshotJson?: string | null;
  createdAt: string;
  messageCount: number;
}

export interface AiConversationMessage {
  id: string;
  conversationId: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  contextJson?: string | null;
  tokensUsed: number;
  modelUsed?: string | null;
  createdAtUtc: string;
}

export interface AiChatRequest {
  conversationId?: string | null;
  message: string;
  currentPageContext?: string | null;
}

export interface AiChatResponse {
  conversationId: string;
  message: AiConversationMessage;
  isFallbackDiagnostic: boolean;
}

export interface BusinessQueryRequest {
  query: string;
  period?: string | null;
}

export interface BusinessQueryResult {
  query: string;
  metricName: string;
  answerText: string;
  period: string;
  columns: string[];
  rows: Record<string, any>[];
  chartType?: 'bar' | 'line' | 'pie' | 'donut' | null;
  chartDataJson?: string | null;
  summaryNote?: string | null;
  success: boolean;
}

export interface AiProviderConfig {
  id: string;
  providerType: string;
  endpointUrl?: string | null;
  modelName: string;
  temperature: number;
  maxTokens: number;
  isActive: boolean;
  hasApiKey: boolean;
}

export interface UpdateAiProviderConfigRequest {
  providerType: string;
  endpointUrl?: string | null;
  modelName: string;
  apiKey?: string | null;
  temperature: number;
  maxTokens: number;
  isActive: boolean;
}

export interface AiUsageLog {
  id: string;
  userId: string;
  operationType: string;
  providerType: string;
  modelName: string;
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  durationMs: number;
  success: boolean;
  errorMessage?: string | null;
  timestamp: string;
}

export interface AiUsageSummary {
  totalRequests: number;
  totalTokens: number;
  totalEstimatedCostUsd: number;
  requestsByOperation: Record<string, number>;
  recentLogs: AiUsageLog[];
}

export interface SalesAnalysisSummary {
  overviewText: string;
  strengths: string[];
  weaknesses: string[];
  actionableRecommendations: string[];
  currentMonthWinRate: number;
  pipelineHealthScore: number;
}

export interface ForecastDataPoint {
  dateLabel: string;
  amount: number;
  isForecast: boolean;
}

export interface RevenueForecast {
  id: string;
  forecastDate: string;
  horizonDays: number;
  periodLabel: string;
  predictedAmount: number;
  lowerBound: number;
  upperBound: number;
  confidenceScore: number;
  assumptionsJson?: string | null;
  modelType: string;
  historicalTrend: ForecastDataPoint[];
  forecastPoints: ForecastDataPoint[];
}

export interface AnomalyDetection {
  id: string;
  metricName: string;
  category: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  actualValue: number;
  expectedValue: number;
  deviationPercent: number;
  description: string;
  detectedAt: string;
  status: 'Active' | 'Investigating' | 'Resolved' | 'FalsePositive';
  resolutionNotes?: string | null;
}

export interface ResolveAnomalyRequest {
  status: string;
  resolutionNotes?: string;
}

export interface BusinessRecommendation {
  id: string;
  category: string;
  targetEntityType: string;
  targetEntityId: string;
  targetEntityName: string;
  title: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  status: 'Pending' | 'Applied' | 'Dismissed';
  generatedAt: string;
  actionPayloadJson?: string | null;
}

export interface ScheduledReport {
  id: string;
  name: string;
  reportType: 'Daily' | 'Weekly' | 'Monthly' | 'AdHoc';
  recipientsEmails: string[];
  format: 'PDF' | 'Excel' | 'InApp';
  timeOfDay: string;
  dayOfWeek?: number | null;
  dayOfMonth?: number | null;
  isActive: boolean;
  lastRunAt?: string | null;
}

export interface GeneratedReport {
  id: string;
  scheduledReportId?: string | null;
  reportType: string;
  title: string;
  periodStart: string;
  periodEnd: string;
  summaryJson: string;
  fullDataJson: string;
  fileUrl?: string | null;
  generatedAt: string;
}

export interface RepPerformanceReportItem {
  employeeName: string;
  teamName: string;
  achievedRevenue: number;
  wonDeals: number;
  kpiAchievementRate: number;
}

export interface ManagementReportData {
  title: string;
  periodLabel: string;
  totalRevenue: number;
  totalCollectedCash: number;
  totalReceivables: number;
  newLeadsCount: number;
  qualifiedLeadsCount: number;
  dealsCreatedCount: number;
  dealsWonCount: number;
  dealsWonValue: number;
  activeContractsCount: number;
  activeContractsValue: number;
  totalPayrollPaid: number;
  topPerformers: RepPerformanceReportItem[];
}

export interface DealSummaryItem {
  id: string;
  title: string;
  accountName: string;
  amount: number;
  stageName: string;
  probability: number;
  expectedCloseDate?: string | null;
}

export interface TaskSummaryItem {
  id: string;
  title: string;
  priority: string;
  dueDate?: string | null;
  status: string;
}

export interface SalesDashboard {
  employeeId: string;
  employeeName: string;
  assignedLeadsCount: number;
  activeDealsCount: number;
  pipelineValue: number;
  targetRevenue: number;
  achievedRevenue: number;
  revenueAttainmentPercent: number;
  wonDealsCount: number;
  pendingTasksCount: number;
  todayAppointmentsCount: number;
  estimatedCommission: number;
  keyDeals: DealSummaryItem[];
  urgentTasks: TaskSummaryItem[];
}

export interface ManagerDashboard {
  managerId: string;
  departmentName: string;
  teamMembersCount: number;
  teamTargetRevenue: number;
  teamAchievedRevenue: number;
  attainmentPercent: number;
  totalPipelineValue: number;
  totalWonDealsCount: number;
  unassignedLeadsCount: number;
  stagnantDealsCount: number;
  teamWorkloads: {
    employeeId: string;
    employeeName: string;
    employeeCode: string;
    teamName?: string | null;
    activeLeadsCount: number;
    maxCapacity: number;
    capacityUtilizationPercent: number;
    isAvailable: boolean;
  }[];
  leaderboard: RepPerformanceReportItem[];
}

export interface ExecutiveDashboard {
  totalRevenue: number;
  totalCollectedCash: number;
  totalReceivables: number;
  overdueDebtAmount: number;
  totalPipelineValue: number;
  totalDealsInPipeline: number;
  totalWonDealsYearToDate: number;
  winRatePercent: number;
  grossMarginPercent: number;
  currentMonthPayrollTotal: number;
  receivablesAging: Record<string, number>;
  topDealsClosingSoon: DealSummaryItem[];
  activeCriticalAnomalies: AnomalyDetection[];
  highPriorityRecommendations: BusinessRecommendation[];
}
