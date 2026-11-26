export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  status: 'Active' | 'Inactive' | 'Suspended' | 'PendingActivation';
  lastLoginAt?: string | null;
  roles: string[];
  departmentName?: string | null;
  teamName?: string | null;
  position?: string | null;
  createdAt: string;
}

export interface UserDetail {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  status: 'Active' | 'Inactive' | 'Suspended' | 'PendingActivation';
  failedLoginAttempts: number;
  lastLoginAt?: string | null;
  roles: string[];
  departmentId?: string | null;
  departmentName?: string | null;
  teamId?: string | null;
  teamName?: string | null;
  position?: string | null;
  phone?: string | null;
  baseSalary: number;
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  roles: string[];
  departmentId?: string | null;
  teamId?: string | null;
  position?: string | null;
  phone?: string | null;
  baseSalary?: number | null;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  position?: string | null;
  phone?: string | null;
  departmentId?: string | null;
  teamId?: string | null;
  baseSalary?: number | null;
  status?: string | null;
}

export interface UserSession {
  id: string;
  userId: string;
  userEmail: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAtUtc: string;
  expiresAtUtc: string;
  isRevoked: boolean;
  revokedAtUtc?: string | null;
}

export interface EmployeeHierarchy {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  employeeCode: string;
  position: string;
  phone: string;
  isActive: boolean;
  teamId?: string | null;
  teamName?: string | null;
}

export interface TeamHierarchy {
  id: string;
  name: string;
  teamLeaderId?: string | null;
  teamLeaderName?: string | null;
  memberCount: number;
  members: EmployeeHierarchy[];
}

export interface DepartmentHierarchy {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  managerId?: string | null;
  managerName?: string | null;
  totalEmployees: number;
  teams: TeamHierarchy[];
  directEmployees: EmployeeHierarchy[];
}

export interface TransferEmployeeRequest {
  employeeId: string;
  newDepartmentId: string;
  newTeamId?: string | null;
  newPosition?: string | null;
  newBaseSalary?: number | null;
  effectiveDate: string;
  reason: string;
}

export interface OrganizationChangeHistory {
  id: string;
  employeeId: string;
  employeeName: string;
  changeType: string;
  oldDepartmentName?: string | null;
  newDepartmentName?: string | null;
  oldTeamName?: string | null;
  newTeamName?: string | null;
  oldPosition?: string | null;
  newPosition?: string | null;
  oldBaseSalary?: number | null;
  newBaseSalary?: number | null;
  effectiveDate: string;
  reason: string;
  createdAtUtc: string;
}

export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  usersCount: number;
  permissions: string[];
}

export interface CreateRoleRequest {
  name: string;
  description: string;
  permissions: string[];
}

export interface UpdateRoleRequest {
  description: string;
  permissions: string[];
}

export interface PermissionDto {
  id: string;
  code: string;
  module: string;
  description: string;
}

export interface RolePermissionEntry {
  roleName: string;
  hasPermission: boolean;
  dataScope: string;
}

export interface PermissionMatrixRow {
  permissionCode: string;
  module: string;
  description: string;
  roles: Record<string, RolePermissionEntry>;
}

export interface RolePermissionMatrix {
  roleNames: string[];
  rows: PermissionMatrixRow[];
}

export interface AuditLog {
  id: string;
  userId?: string | null;
  userEmail: string;
  action: string;
  module: string;
  entityName: string;
  entityId?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
  isSuccess: boolean;
  timestamp: string;
}

export interface AuditLogDetail extends AuditLog {
  correlationId?: string | null;
  oldValuesJson?: string | null;
  newValuesJson?: string | null;
}

export interface DataImportJob {
  id: string;
  entityType: string;
  fileName: string;
  totalRecords: number;
  processedRecords: number;
  successCount: number;
  failureCount: number;
  status: string;
  errorLogJson?: string | null;
  uploadedByEmail: string;
  createdAtUtc: string;
  completedAtUtc?: string | null;
}

export interface DataExportJob {
  id: string;
  entityType: string;
  format: string;
  totalRecords: number;
  status: string;
  fileUrl?: string | null;
  requestedByEmail: string;
  createdAtUtc: string;
  completedAtUtc?: string | null;
}

export interface ImportExecutionResult {
  jobId: string;
  entityType: string;
  totalRecords: number;
  successCount: number;
  failureCount: number;
  errors: string[];
}

export interface ApiKey {
  id: string;
  name: string;
  keyPrefix: string;
  scopes: string;
  rateLimitPerMinute: number;
  isActive: boolean;
  expiresAtUtc?: string | null;
  lastUsedAtUtc?: string | null;
  createdByEmail: string;
  createdAtUtc: string;
}

export interface ApiKeyCreatedResult {
  id: string;
  name: string;
  plaintextKey: string;
  keyPrefix: string;
  expiresAtUtc?: string | null;
}

export interface CreateApiKeyRequest {
  name: string;
  scopes?: string;
  rateLimitPerMinute?: number;
  expiresInDays?: number;
}

export interface ApiRequestLog {
  id: string;
  apiKeyId?: string | null;
  apiKeyName?: string | null;
  endpoint: string;
  httpMethod: string;
  statusCode: number;
  durationMs: number;
  ipAddress?: string | null;
  timestampUtc: string;
}

export interface WebhookEndpoint {
  id: string;
  name: string;
  url: string;
  subscribedEvents: string[];
  isActive: boolean;
  failureCount: number;
  createdAtUtc: string;
}

export interface WebhookDelivery {
  id: string;
  webhookEndpointId: string;
  eventName: string;
  payloadJson: string;
  responseStatusCode?: number | null;
  responseBody?: string | null;
  durationMs: number;
  attemptCount: number;
  status: string;
  nextRetryAtUtc?: string | null;
  createdAtUtc: string;
}

export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  link?: string | null;
  isRead: boolean;
  type: string;
  category: string;
  priority: string;
  createdAt: string;
}

export interface NotificationSummary {
  totalCount: number;
  unreadCount: number;
  recentNotifications: NotificationItem[];
}

export interface BackupRecord {
  id: string;
  backupName: string;
  backupType: string;
  status: string;
  fileSizeBytes: number;
  fileSizeFormatted: string;
  storagePath?: string | null;
  checksumSha256?: string | null;
  notes?: string | null;
  initiatedByEmail?: string | null;
  createdAtUtc: string;
  completedAtUtc?: string | null;
}

export interface RestoreRecord {
  id: string;
  backupRecordId: string;
  backupName: string;
  status: string;
  targetEnvironment: string;
  executionLog?: string | null;
  verifiedByEmail: string;
  createdAtUtc: string;
  completedAtUtc?: string | null;
}

export interface SupabasePitrStatus {
  isPitrEnabled: boolean;
  planTier: string;
  retentionDays: number;
  earliestRestorePointUtc: string;
  latestRestorePointUtc: string;
  statusMessage: string;
}

export interface SystemConfiguration {
  id: string;
  category: string;
  key: string;
  value: string;
  description?: string | null;
  isEncrypted: boolean;
  updatedByEmail?: string | null;
  updatedAtUtc: string;
}

export interface ConfigurationHistory {
  id: string;
  configurationId: string;
  oldValue?: string | null;
  newValue: string;
  changedByEmail?: string | null;
  timestampUtc: string;
}
