import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  DealDto,
  PagedResult,
  CreateDealRequest,
  UpdateDealRequest,
  UpdateDealStageRequest,
  KanbanBoardDto,
  DealStageHistoryDto,
} from '../types/crm';

export const dealService = {
  async getDeals(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    accountId?: string;
    stageId?: string;
    status?: string;
    assignedEmployeeId?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }): Promise<ApiResponse<PagedResult<DealDto>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<DealDto>>>('/deals', { params });
    return res.data;
  },

  async getDealById(id: string): Promise<ApiResponse<DealDto>> {
    const res = await apiClient.get<ApiResponse<DealDto>>(`/deals/${id}`);
    return res.data;
  },

  async getKanbanBoard(params?: {
    assignedEmployeeId?: string;
    status?: string;
  }): Promise<ApiResponse<KanbanBoardDto>> {
    const res = await apiClient.get<ApiResponse<KanbanBoardDto>>('/deals/kanban', { params });
    return res.data;
  },

  async getDealStageHistory(id: string): Promise<ApiResponse<DealStageHistoryDto[]>> {
    const res = await apiClient.get<ApiResponse<DealStageHistoryDto[]>>(`/deals/${id}/history`);
    return res.data;
  },

  async createDeal(data: CreateDealRequest): Promise<ApiResponse<DealDto>> {
    const res = await apiClient.post<ApiResponse<DealDto>>('/deals', data);
    return res.data;
  },

  async updateDeal(id: string, data: UpdateDealRequest): Promise<ApiResponse<DealDto>> {
    const res = await apiClient.put<ApiResponse<DealDto>>(`/deals/${id}`, data);
    return res.data;
  },

  async updateDealStage(id: string, data: UpdateDealStageRequest): Promise<ApiResponse<DealDto>> {
    const res = await apiClient.put<ApiResponse<DealDto>>(`/deals/${id}/stage`, data);
    return res.data;
  },

  async deleteDeal(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/deals/${id}`);
    return res.data;
  },
};
