import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type { PagedResult } from '../types/crm';
import type { Contract, ContractAppendix, ContractStatus } from '../types/billing';

export interface CreateContractRequest {
  title: string;
  accountId: string;
  contactId?: string;
  dealId?: string;
  quotationId?: string;
  templateId?: string;
  startDate: string;
  endDate: string;
  totalValue: number;
  scopeOfWork?: string;
  paymentTerms?: string;
  specialTerms?: string;
  items?: Array<{
    serviceId?: string;
    serviceName: string;
    quantity: number;
    unitPrice: number;
    notes?: string;
  }>;
}

export interface UpdateContractRequest {
  title: string;
  contactId?: string;
  status: ContractStatus;
  startDate: string;
  endDate: string;
  totalValue: number;
  scopeOfWork?: string;
  paymentTerms?: string;
  specialTerms?: string;
  items?: Array<{
    serviceId?: string;
    serviceName: string;
    quantity: number;
    unitPrice: number;
    notes?: string;
  }>;
}

export interface CreateContractFromQuotationRequest {
  quotationId: string;
  templateId?: string;
  title?: string;
  startDate?: string;
  endDate?: string;
}

export interface AddAppendixRequest {
  title: string;
  description?: string;
  adjustedValue: number;
  effectiveDate: string;
  status: string;
}

export interface RenewContractRequest {
  newStartDate: string;
  newEndDate: string;
  newTotalValue?: number;
  renewalNotes?: string;
}

export const contractService = {
  async getContracts(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    accountId?: string;
    status?: string;
  }): Promise<ApiResponse<PagedResult<Contract>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<Contract>>>('/contracts', { params });
    return res.data;
  },

  async getContractById(id: string): Promise<ApiResponse<Contract>> {
    const res = await apiClient.get<ApiResponse<Contract>>(`/contracts/${id}`);
    return res.data;
  },

  async createContract(data: CreateContractRequest): Promise<ApiResponse<Contract>> {
    const res = await apiClient.post<ApiResponse<Contract>>('/contracts', data);
    return res.data;
  },

  async createFromQuotation(data: CreateContractFromQuotationRequest): Promise<ApiResponse<Contract>> {
    const res = await apiClient.post<ApiResponse<Contract>>('/contracts/from-quotation', data);
    return res.data;
  },

  async updateContract(id: string, data: UpdateContractRequest): Promise<ApiResponse<Contract>> {
    const res = await apiClient.put<ApiResponse<Contract>>(`/contracts/${id}`, data);
    return res.data;
  },

  async deleteContract(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/contracts/${id}`);
    return res.data;
  },

  async getContractContent(id: string): Promise<ApiResponse<string>> {
    const res = await apiClient.get<ApiResponse<string>>(`/contracts/${id}/content`);
    return res.data;
  },

  async addAppendix(contractId: string, data: AddAppendixRequest): Promise<ApiResponse<ContractAppendix>> {
    const res = await apiClient.post<ApiResponse<ContractAppendix>>(`/contracts/${contractId}/appendices`, data);
    return res.data;
  },

  async renewContract(contractId: string, data: RenewContractRequest): Promise<ApiResponse<Contract>> {
    const res = await apiClient.post<ApiResponse<Contract>>(`/contracts/${contractId}/renew`, data);
    return res.data;
  },

  async downloadPdf(id: string, contractNumber: string): Promise<void> {
    const res = await apiClient.get(`/contracts/${id}/pdf`, {
      responseType: 'blob',
    });
    const url = window.URL.createObjectURL(new Blob([res.data], { type: 'application/pdf' }));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `${contractNumber}.pdf`);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },
};
