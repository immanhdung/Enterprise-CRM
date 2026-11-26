export interface WorkflowDefinition {
  id: string;
  name: string;
  description: string;
  category: string;
  triggerEvent: string;
  conditionsJson?: string | null;
  actionsJson?: string | null;
  executionOrder: number;
  version: number;
  isActive: boolean;
  executionCount: number;
  errorCount: number;
  lastExecutedAt?: string | null;
  createdAt: string;
}

export interface CreateWorkflowRequest {
  name: string;
  description: string;
  category: string;
  triggerEvent: string;
  conditionsJson?: string | null;
  actionsJson?: string | null;
  executionOrder: number;
  isActive: boolean;
}

export interface WorkflowExecutionLog {
  id: string;
  workflowExecutionId: string;
  stepName: string;
  actionType: string;
  status: 'Success' | 'Failed' | 'Skipped';
  inputJson?: string | null;
  outputJson?: string | null;
  errorMessage?: string | null;
  executedAt: string;
}

export interface WorkflowExecution {
  id: string;
  workflowId: string;
  workflowName: string;
  triggerEvent: string;
  entityType: string;
  entityId: string;
  status: 'Success' | 'Failed' | 'Partial' | 'Skipped';
  executedAt: string;
  durationMs: number;
  resultSummary?: string | null;
  errorMessage?: string | null;
  logs: WorkflowExecutionLog[];
}

export interface LeadAssignmentRule {
  id: string;
  name: string;
  description: string;
  priority: number;
  mode: 'RoundRobin' | 'Workload' | 'Territory' | 'Source' | 'ServiceSpecialization';
  conditionsJson?: string | null;
  targetEmployeeIdsJson?: string | null;
  targetTeamIdsJson?: string | null;
  maxActiveLeadsPerRep?: number | null;
  isActive: boolean;
}

export interface CreateLeadAssignmentRuleRequest {
  name: string;
  description: string;
  priority: number;
  mode: string;
  conditionsJson?: string | null;
  targetEmployeeIdsJson?: string | null;
  targetTeamIdsJson?: string | null;
  maxActiveLeadsPerRep?: number | null;
  isActive: boolean;
}

export interface LeadAssignmentLog {
  id: string;
  leadId: string;
  leadName: string;
  ruleId?: string | null;
  ruleName?: string | null;
  previousEmployeeId?: string | null;
  previousEmployeeName?: string | null;
  assignedEmployeeId?: string | null;
  assignedEmployeeName?: string | null;
  mode: string;
  reason: string;
  assignedByUserId?: string | null;
  timestamp: string;
}

export interface RepWorkload {
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  teamName?: string | null;
  activeLeadsCount: number;
  maxCapacity: number;
  capacityUtilizationPercent: number;
  isAvailable: boolean;
}

export interface LeadDistributionMetrics {
  totalAssignedToday: number;
  totalAssignedThisMonth: number;
  totalUnassignedLeads: number;
  distributionByMode: Record<string, number>;
  repWorkloads: RepWorkload[];
}

export interface AlertRule {
  id: string;
  name: string;
  category: string;
  alertType: string;
  thresholdDays: number;
  thresholdAmount?: number | null;
  severity: 'Info' | 'Warning' | 'Critical';
  channels: string;
  isActive: boolean;
  description?: string | null;
}

export interface BusinessAlert {
  id: string;
  alertRuleId?: string | null;
  title: string;
  message: string;
  category: string;
  entityType: string;
  entityId: string;
  severity: 'Info' | 'Warning' | 'Critical';
  status: 'Active' | 'Acknowledged' | 'Resolved' | 'Dismissed';
  assignedToUserId?: string | null;
  triggeredAt: string;
  acknowledgedAt?: string | null;
  acknowledgedBy?: string | null;
  resolvedAt?: string | null;
  resolvedBy?: string | null;
  resolutionNotes?: string | null;
}

export interface NotificationPreference {
  id: string;
  userId: string;
  category: string;
  inAppEnabled: boolean;
  emailEnabled: boolean;
  minSeverity: string;
}
