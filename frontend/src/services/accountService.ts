import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  AccountDto,
  AccountDetailDto,
  PagedResult,
  CreateAccountRequest,
  UpdateAccountRequest,
} from '../types/crm';

export const accountService = {
  async getAccounts(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    industry?: string;
    classification?: string;
    status?: string;
    assignedEmployeeId?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }): Promise<ApiResponse<PagedResult<AccountDto>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<AccountDto>>>('/accounts', { params });
    return res.data;
  },

  async getAccountById(id: string): Promise<ApiResponse<AccountDetailDto>> {
    const res = await apiClient.get<ApiResponse<AccountDetailDto>>(`/accounts/${id}`);
    return res.data;
  },

  async createAccount(data: CreateAccountRequest): Promise<ApiResponse<AccountDto>> {
    const res = await apiClient.post<ApiResponse<AccountDto>>('/accounts', data);
    return res.data;
  },

  async updateAccount(id: string, data: UpdateAccountRequest): Promise<ApiResponse<AccountDto>> {
    const res = await apiClient.put<ApiResponse<AccountDto>>(`/accounts/${id}`, data);
    return res.data;
  },

  async deleteAccount(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/accounts/${id}`);
    return res.data;
  },
};
