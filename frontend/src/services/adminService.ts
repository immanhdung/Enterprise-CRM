import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  UserSummary,
  UserDetail,
  CreateUserRequest,
  UpdateUserRequest,
  UserSession,
  DepartmentHierarchy,
  TransferEmployeeRequest,
  OrganizationChangeHistory,
  RoleDto,
  CreateRoleRequest,
  UpdateRoleRequest,
  PermissionDto,
  RolePermissionMatrix,
  AuditLog,
  AuditLogDetail,
  DataImportJob,
  DataExportJob,
  ImportExecutionResult,
  ApiKey,
  ApiKeyCreatedResult,
  CreateApiKeyRequest,
  ApiRequestLog,
  WebhookEndpoint,
  WebhookDelivery,
  NotificationSummary,
  NotificationItem,
  BackupRecord,
  RestoreRecord,
  SupabasePitrStatus,
  SystemConfiguration,
  ConfigurationHistory,
} from '../types/admin';

export const adminService = {
  // ==========================================
  // 1. User Administration
  // ==========================================
  async getUsers(params?: { search?: string; status?: string; departmentId?: string; role?: string }): Promise<UserSummary[]> {
    const sp = new URLSearchParams();
    if (params?.search) sp.append('search', params.search);
    if (params?.status) sp.append('status', params.status);
    if (params?.departmentId) sp.append('departmentId', params.departmentId);
    if (params?.role) sp.append('role', params.role);

    const res = await apiClient.get<ApiResponse<UserSummary[]>>(`/admin/users?${sp.toString()}`);
    return res.data.data || [];
  },

  async getUserById(id: string): Promise<UserDetail> {
    const res = await apiClient.get<ApiResponse<UserDetail>>(`/admin/users/${id}`);
    return res.data.data!;
  },

  async createUser(request: CreateUserRequest): Promise<UserDetail> {
    const res = await apiClient.post<ApiResponse<UserDetail>>('/admin/users', request);
    return res.data.data!;
  },

  async updateUser(id: string, request: UpdateUserRequest): Promise<UserDetail> {
    const res = await apiClient.put<ApiResponse<UserDetail>>(`/admin/users/${id}`, request);
    return res.data.data!;
  },

  async updateStatus(id: string, status: string): Promise<boolean> {
    const res = await apiClient.patch<ApiResponse<boolean>>(`/admin/users/${id}/status`, { status });
    return res.data.data || false;
  },

  async resetPassword(id: string, newPassword: string): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/admin/users/${id}/reset-password`, { newPassword });
    return res.data.data || false;
  },

  async assignRoles(id: string, roles: string[]): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/admin/users/${id}/roles`, { roles });
    return res.data.data || false;
  },

  async getSessions(userId?: string): Promise<UserSession[]> {
    const sp = new URLSearchParams();
    if (userId) sp.append('userId', userId);
    const res = await apiClient.get<ApiResponse<UserSession[]>>(`/admin/users/sessions?${sp.toString()}`);
    return res.data.data || [];
  },

  async revokeSession(sessionId: string): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/admin/users/sessions/${sessionId}/revoke`);
    return res.data.data || false;
  },

  // ==========================================
  // 2. Organization Management
  // ==========================================
  async getHierarchy(): Promise<DepartmentHierarchy[]> {
    const res = await apiClient.get<ApiResponse<DepartmentHierarchy[]>>('/admin/organization/hierarchy');
    return res.data.data || [];
  },

  async transferEmployee(request: TransferEmployeeRequest): Promise<OrganizationChangeHistory> {
    const res = await apiClient.post<ApiResponse<OrganizationChangeHistory>>('/admin/organization/transfer', request);
    return res.data.data!;
  },

  async getChangeHistory(employeeId?: string): Promise<OrganizationChangeHistory[]> {
    const sp = new URLSearchParams();
    if (employeeId) sp.append('employeeId', employeeId);
    const res = await apiClient.get<ApiResponse<OrganizationChangeHistory[]>>(`/admin/organization/history?${sp.toString()}`);
    return res.data.data || [];
  },

  // ==========================================
  // 3. RBAC & Permissions
  // ==========================================
  async getRoles(): Promise<RoleDto[]> {
    const res = await apiClient.get<ApiResponse<RoleDto[]>>('/admin/rbac/roles');
    return res.data.data || [];
  },

  async getRoleById(id: string): Promise<RoleDto> {
    const res = await apiClient.get<ApiResponse<RoleDto>>(`/admin/rbac/roles/${id}`);
    return res.data.data!;
  },

  async createRole(request: CreateRoleRequest): Promise<RoleDto> {
    const res = await apiClient.post<ApiResponse<RoleDto>>('/admin/rbac/roles', request);
    return res.data.data!;
  },

  async updateRole(id: string, request: UpdateRoleRequest): Promise<RoleDto> {
    const res = await apiClient.put<ApiResponse<RoleDto>>(`/admin/rbac/roles/${id}`, request);
    return res.data.data!;
  },

  async deleteRole(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/admin/rbac/roles/${id}`);
    return res.data.data || false;
  },

  async getPermissions(): Promise<PermissionDto[]> {
    const res = await apiClient.get<ApiResponse<PermissionDto[]>>('/admin/rbac/permissions');
    return res.data.data || [];
  },

  async getPermissionMatrix(): Promise<RolePermissionMatrix> {
    const res = await apiClient.get<ApiResponse<RolePermissionMatrix>>('/admin/rbac/matrix');
    return res.data.data!;
  },

  async updateMatrixEntry(roleName: string, permissionCode: string, isGranted: boolean, dataScope?: string): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>('/admin/rbac/matrix', {
      roleName,
      permissionCode,
      isGranted,
      dataScope,
    });
    return res.data.data || false;
  },

  // ==========================================
  // 4. Audit Logs
  // ==========================================
  async getLogs(params?: { search?: string; module?: string; action?: string; userId?: string; startDate?: string; endDate?: string; pageNumber?: number; pageSize?: number }): Promise<AuditLog[]> {
    const sp = new URLSearchParams();
    if (params?.search) sp.append('search', params.search);
    if (params?.module) sp.append('module', params.module);
    if (params?.action) sp.append('action', params.action);
    if (params?.userId) sp.append('userId', params.userId);
    if (params?.startDate) sp.append('startDate', params.startDate);
    if (params?.endDate) sp.append('endDate', params.endDate);
    if (params?.pageNumber) sp.append('pageNumber', String(params.pageNumber));
    if (params?.pageSize) sp.append('pageSize', String(params.pageSize));

    const res = await apiClient.get<ApiResponse<AuditLog[]>>(`/admin/audit-logs?${sp.toString()}`);
    return res.data.data || [];
  },

  async getLogById(id: string): Promise<AuditLogDetail> {
    const res = await apiClient.get<ApiResponse<AuditLogDetail>>(`/admin/audit-logs/${id}`);
    return res.data.data!;
  },

  // ==========================================
  // 5. Data Management (Import / Export)
  // ==========================================
  async getImportJobs(): Promise<DataImportJob[]> {
    const res = await apiClient.get<ApiResponse<DataImportJob[]>>('/admin/data/import/jobs');
    return res.data.data || [];
  },

  async processImport(entityType: string, fileName: string, rows: Record<string, any>[]): Promise<ImportExecutionResult> {
    const res = await apiClient.post<ApiResponse<ImportExecutionResult>>('/admin/data/import', {
      entityType,
      fileName,
      rows,
    });
    return res.data.data!;
  },

  async getExportJobs(): Promise<DataExportJob[]> {
    const res = await apiClient.get<ApiResponse<DataExportJob[]>>('/admin/data/export/jobs');
    return res.data.data || [];
  },

  async initiateExport(entityType: string, format = 'Excel', selectedColumns?: string[], filterJson?: string): Promise<DataExportJob> {
    const res = await apiClient.post<ApiResponse<DataExportJob>>('/admin/data/export', {
      entityType,
      format,
      selectedColumns,
      filterJson,
    });
    return res.data.data!;
  },

  // ==========================================
  // 6. Integrations (API Keys & Webhooks)
  // ==========================================
  async getApiKeys(): Promise<ApiKey[]> {
    const res = await apiClient.get<ApiResponse<ApiKey[]>>('/admin/integrations/api-keys');
    return res.data.data || [];
  },

  async createApiKey(request: CreateApiKeyRequest): Promise<ApiKeyCreatedResult> {
    const res = await apiClient.post<ApiResponse<ApiKeyCreatedResult>>('/admin/integrations/api-keys', request);
    return res.data.data!;
  },

  async revokeApiKey(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/admin/integrations/api-keys/${id}`);
    return res.data.data || false;
  },

  async getApiLogs(limit = 50): Promise<ApiRequestLog[]> {
    const res = await apiClient.get<ApiResponse<ApiRequestLog[]>>(`/admin/integrations/api-keys/logs?limit=${limit}`);
    return res.data.data || [];
  },

  async getWebhooks(): Promise<WebhookEndpoint[]> {
    const res = await apiClient.get<ApiResponse<WebhookEndpoint[]>>('/admin/integrations/webhooks');
    return res.data.data || [];
  },

  async getWebhookById(id: string): Promise<WebhookEndpoint> {
    const res = await apiClient.get<ApiResponse<WebhookEndpoint>>(`/admin/integrations/webhooks/${id}`);
    return res.data.data!;
  },

  async createWebhook(name: string, url: string, subscribedEvents: string[]): Promise<WebhookEndpoint> {
    const res = await apiClient.post<ApiResponse<WebhookEndpoint>>('/admin/integrations/webhooks', {
      name,
      url,
      subscribedEvents,
    });
    return res.data.data!;
  },

  async updateWebhook(id: string, name: string, url: string, subscribedEvents: string[], isActive: boolean): Promise<WebhookEndpoint> {
    const res = await apiClient.put<ApiResponse<WebhookEndpoint>>(`/admin/integrations/webhooks/${id}`, {
      name,
      url,
      subscribedEvents,
      isActive,
    });
    return res.data.data!;
  },

  async deleteWebhook(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/admin/integrations/webhooks/${id}`);
    return res.data.data || false;
  },

  async getWebhookDeliveries(endpointId?: string, limit = 50): Promise<WebhookDelivery[]> {
    const sp = new URLSearchParams();
    if (endpointId) sp.append('endpointId', endpointId);
    sp.append('limit', String(limit));
    const res = await apiClient.get<ApiResponse<WebhookDelivery[]>>(`/admin/integrations/webhooks/deliveries?${sp.toString()}`);
    return res.data.data || [];
  },

  async testWebhook(id: string): Promise<WebhookDelivery> {
    const res = await apiClient.post<ApiResponse<WebhookDelivery>>(`/admin/integrations/webhooks/${id}/test`);
    return res.data.data!;
  },

  // ==========================================
  // 7. Notification Center
  // ==========================================
  async getNotifications(unreadOnly?: boolean, limit = 50): Promise<NotificationSummary> {
    const sp = new URLSearchParams();
    if (unreadOnly !== undefined) sp.append('unreadOnly', String(unreadOnly));
    sp.append('limit', String(limit));
    const res = await apiClient.get<ApiResponse<NotificationSummary>>(`/notifications?${sp.toString()}`);
    return res.data.data!;
  },

  async markAsRead(id: string): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/notifications/${id}/read`);
    return res.data.data || false;
  },

  async markAllAsRead(): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>('/notifications/read-all');
    return res.data.data || false;
  },

  async deleteNotification(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/notifications/${id}`);
    return res.data.data || false;
  },

  async dispatchNotification(request: { recipientUserId?: string; title: string; message: string; link?: string; type?: string; category?: string; priority?: string }): Promise<NotificationItem> {
    const res = await apiClient.post<ApiResponse<NotificationItem>>('/notifications/dispatch', request);
    return res.data.data!;
  },

  // ==========================================
  // 8. Backup & Restore
  // ==========================================
  async getBackups(): Promise<BackupRecord[]> {
    const res = await apiClient.get<ApiResponse<BackupRecord[]>>('/admin/backups');
    return res.data.data || [];
  },

  async initiateBackup(backupName?: string, notes?: string): Promise<BackupRecord> {
    const res = await apiClient.post<ApiResponse<BackupRecord>>('/admin/backups', { backupName, notes });
    return res.data.data!;
  },

  async getRestores(): Promise<RestoreRecord[]> {
    const res = await apiClient.get<ApiResponse<RestoreRecord[]>>('/admin/backups/restores');
    return res.data.data || [];
  },

  async executeRestoreDrill(backupRecordId: string, targetEnvironment = 'SandboxVerification'): Promise<RestoreRecord> {
    const res = await apiClient.post<ApiResponse<RestoreRecord>>('/admin/backups/restore-drill', { backupRecordId, targetEnvironment });
    return res.data.data!;
  },

  async getSupabasePitrStatus(): Promise<SupabasePitrStatus> {
    const res = await apiClient.get<ApiResponse<SupabasePitrStatus>>('/admin/backups/supabase-pitr');
    return res.data.data!;
  },

  // ==========================================
  // 9. System Settings
  // ==========================================
  async getSettings(category?: string): Promise<SystemConfiguration[]> {
    const sp = new URLSearchParams();
    if (category) sp.append('category', category);
    const res = await apiClient.get<ApiResponse<SystemConfiguration[]>>(`/admin/settings?${sp.toString()}`);
    return res.data.data || [];
  },

  async updateSetting(category: string, key: string, value: string, description?: string): Promise<SystemConfiguration> {
    const res = await apiClient.put<ApiResponse<SystemConfiguration>>('/admin/settings', { category, key, value, description });
    return res.data.data!;
  },

  async getSettingHistory(id: string): Promise<ConfigurationHistory[]> {
    const res = await apiClient.get<ApiResponse<ConfigurationHistory[]>>(`/admin/settings/${id}/history`);
    return res.data.data || [];
  },
};
