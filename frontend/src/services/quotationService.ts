import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type { PagedResult } from '../types/crm';
import type { Quotation } from '../types/billing';

export interface QuotationItemInput {
  serviceId?: string;
  serviceCode: string;
  ServiceName: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  vatRate: number;
  notes?: string;
}

export interface CreateQuotationRequest {
  dealId?: string;
  accountId: string;
  contactId?: string;
  priceListId?: string;
  validUntil: string;
  termsAndConditions?: string;
  notes?: string;
  items: QuotationItemInput[];
}

export interface UpdateQuotationRequest {
  contactId?: string;
  priceListId?: string;
  validUntil: string;
  termsAndConditions?: string;
  notes?: string;
  items: QuotationItemInput[];
}

export const quotationService = {
  async getQuotations(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    accountId?: string;
    dealId?: string;
    status?: string;
  }): Promise<ApiResponse<PagedResult<Quotation>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<Quotation>>>('/quotations', { params });
    return res.data;
  },

  async getQuotationById(id: string): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.get<ApiResponse<Quotation>>(`/quotations/${id}`);
    return res.data;
  },

  async createQuotation(data: CreateQuotationRequest): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.post<ApiResponse<Quotation>>('/quotations', data);
    return res.data;
  },

  async updateQuotation(id: string, data: UpdateQuotationRequest): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.put<ApiResponse<Quotation>>(`/quotations/${id}`, data);
    return res.data;
  },

  async deleteQuotation(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/quotations/${id}`);
    return res.data;
  },

  async submitForApproval(id: string, notes?: string): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.post<ApiResponse<Quotation>>(`/quotations/${id}/submit`, { notes });
    return res.data;
  },

  async approveQuotation(id: string, approvalNotes?: string): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.post<ApiResponse<Quotation>>(`/quotations/${id}/approve`, { approvalNotes });
    return res.data;
  },

  async rejectQuotation(id: string, reason: string): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.post<ApiResponse<Quotation>>(`/quotations/${id}/reject`, { reason });
    return res.data;
  },

  async createRevision(id: string, changeNotes: string): Promise<ApiResponse<Quotation>> {
    const res = await apiClient.post<ApiResponse<Quotation>>(`/quotations/${id}/revise`, { changeNotes });
    return res.data;
  },

  async getVersions(id: string): Promise<ApiResponse<Quotation[]>> {
    const res = await apiClient.get<ApiResponse<Quotation[]>>(`/quotations/${id}/versions`);
    return res.data;
  },

  async downloadPdf(id: string, quotationNumber: string, revisionNumber: string): Promise<void> {
    const res = await apiClient.get(`/quotations/${id}/pdf`, {
      responseType: 'blob',
    });
    const url = window.URL.createObjectURL(new Blob([res.data], { type: 'application/pdf' }));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `${quotationNumber}_${revisionNumber}.pdf`);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  async sendEmail(id: string, recipientEmail: string, emailSubject?: string, emailBody?: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/quotations/${id}/send-email`, {
      recipientEmail,
      emailSubject,
      emailBody,
    });
    return res.data;
  },
};
