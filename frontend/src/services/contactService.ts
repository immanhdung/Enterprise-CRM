import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  ContactDto,
  PagedResult,
  CreateContactRequest,
  UpdateContactRequest,
} from '../types/crm';

export const contactService = {
  async getContacts(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    accountId?: string;
    isPrimary?: boolean;
    status?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }): Promise<ApiResponse<PagedResult<ContactDto>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<ContactDto>>>('/contacts', { params });
    return res.data;
  },

  async getContactById(id: string): Promise<ApiResponse<ContactDto>> {
    const res = await apiClient.get<ApiResponse<ContactDto>>(`/contacts/${id}`);
    return res.data;
  },

  async createContact(data: CreateContactRequest): Promise<ApiResponse<ContactDto>> {
    const res = await apiClient.post<ApiResponse<ContactDto>>('/contacts', data);
    return res.data;
  },

  async updateContact(id: string, data: UpdateContactRequest): Promise<ApiResponse<ContactDto>> {
    const res = await apiClient.put<ApiResponse<ContactDto>>(`/contacts/${id}`, data);
    return res.data;
  },

  async deleteContact(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/contacts/${id}`);
    return res.data;
  },
};
