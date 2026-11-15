import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  LeadDto,
  PagedResult,
  CreateLeadRequest,
  UpdateLeadRequest,
  AssignLeadRequest,
  ConvertLeadRequest,
  DuplicateCheckResult,
  LeadActivityDto,
} from '../types/crm';

export const leadService = {
  async getLeads(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    status?: string;
    source?: string;
    priority?: string;
    assignedEmployeeId?: string;
    createdFrom?: string;
    createdTo?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }): Promise<ApiResponse<PagedResult<LeadDto>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<LeadDto>>>('/leads', { params });
    return res.data;
  },

  async getLeadById(id: string): Promise<ApiResponse<LeadDto>> {
    const res = await apiClient.get<ApiResponse<LeadDto>>(`/leads/${id}`);
    return res.data;
  },

  async checkDuplicates(params: { email?: string; phone?: string; excludeId?: string }): Promise<ApiResponse<DuplicateCheckResult>> {
    const res = await apiClient.get<ApiResponse<DuplicateCheckResult>>('/leads/check-duplicate', { params });
    return res.data;
  },

  async createLead(data: CreateLeadRequest): Promise<ApiResponse<LeadDto>> {
    const res = await apiClient.post<ApiResponse<LeadDto>>('/leads', data);
    return res.data;
  },

  async updateLead(id: string, data: UpdateLeadRequest): Promise<ApiResponse<LeadDto>> {
    const res = await apiClient.put<ApiResponse<LeadDto>>(`/leads/${id}`, data);
    return res.data;
  },

  async updateLeadStatus(id: string, status: string): Promise<ApiResponse<LeadDto>> {
    const res = await apiClient.patch<ApiResponse<LeadDto>>(`/leads/${id}/status`, null, { params: { status } });
    return res.data;
  },

  async assignLead(id: string, data: AssignLeadRequest): Promise<ApiResponse<LeadDto>> {
    const res = await apiClient.post<ApiResponse<LeadDto>>(`/leads/${id}/assign`, data);
    return res.data;
  },

  async convertLead(id: string, data: ConvertLeadRequest): Promise<ApiResponse<{
    accountId: string;
    accountName: string;
    contactId: string;
    contactName: string;
    dealId: string;
    dealTitle: string;
    convertedAt: string;
  }>> {
    const res = await apiClient.post<ApiResponse<{
      accountId: string;
      accountName: string;
      contactId: string;
      contactName: string;
      dealId: string;
      dealTitle: string;
      convertedAt: string;
    }>>(`/leads/${id}/convert`, data);
    return res.data;
  },

  async deleteLead(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/leads/${id}`);
    return res.data;
  },

  async getLeadActivities(id: string): Promise<ApiResponse<LeadActivityDto[]>> {
    const res = await apiClient.get<ApiResponse<LeadActivityDto[]>>(`/leads/${id}/activities`);
    return res.data;
  },

  async logLeadActivity(id: string, data: { type: string; subject: string; notes?: string; scheduledAt?: string }): Promise<ApiResponse<LeadActivityDto>> {
    const res = await apiClient.post<ApiResponse<LeadActivityDto>>(`/leads/${id}/activities`, data);
    return res.data;
  },

  async exportLeads(params?: {
    search?: string;
    status?: string;
    source?: string;
    assignedEmployeeId?: string;
  }): Promise<Blob> {
    const res = await apiClient.get('/leads/export', {
      params,
      responseType: 'blob',
    });
    return res.data;
  },

  async importLeads(file: File): Promise<ApiResponse<{ importedCount: number; errors: string[] }>> {
    const formData = new FormData();
    formData.append('file', file);
    const res = await apiClient.post<ApiResponse<{ importedCount: number; errors: string[] }>>('/leads/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return res.data;
  },
};
