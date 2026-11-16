import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type { ContractTemplate } from '../types/billing';

export interface CreateContractTemplateRequest {
  code: string;
  name: string;
  description?: string;
  contentTemplate: string;
}

export interface UpdateContractTemplateRequest {
  name: string;
  description?: string;
  contentTemplate: string;
  isActive: boolean;
}

export const contractTemplateService = {
  async getTemplates(includeInactive = false): Promise<ApiResponse<ContractTemplate[]>> {
    const res = await apiClient.get<ApiResponse<ContractTemplate[]>>('/contract-templates', {
      params: { includeInactive },
    });
    return res.data;
  },

  async getTemplateById(id: string): Promise<ApiResponse<ContractTemplate>> {
    const res = await apiClient.get<ApiResponse<ContractTemplate>>(`/contract-templates/${id}`);
    return res.data;
  },

  async createTemplate(data: CreateContractTemplateRequest): Promise<ApiResponse<ContractTemplate>> {
    const res = await apiClient.post<ApiResponse<ContractTemplate>>('/contract-templates', data);
    return res.data;
  },

  async updateTemplate(id: string, data: UpdateContractTemplateRequest): Promise<ApiResponse<ContractTemplate>> {
    const res = await apiClient.put<ApiResponse<ContractTemplate>>(`/contract-templates/${id}`, data);
    return res.data;
  },

  async deleteTemplate(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/contract-templates/${id}`);
    return res.data;
  },
};
