import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  TaskDto,
  PagedResult,
  CreateTaskRequest,
  UpdateTaskRequest,
} from '../types/crm';

export const taskService = {
  async getTasks(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    status?: string;
    priority?: string;
    assignedEmployeeId?: string;
    isOverdue?: boolean;
    leadId?: string;
    accountId?: string;
    contactId?: string;
    dealId?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }): Promise<ApiResponse<PagedResult<TaskDto>>> {
    const res = await apiClient.get<ApiResponse<PagedResult<TaskDto>>>('/tasks', { params });
    return res.data;
  },

  async getTaskById(id: string): Promise<ApiResponse<TaskDto>> {
    const res = await apiClient.get<ApiResponse<TaskDto>>(`/tasks/${id}`);
    return res.data;
  },

  async createTask(data: CreateTaskRequest): Promise<ApiResponse<TaskDto>> {
    const res = await apiClient.post<ApiResponse<TaskDto>>('/tasks', data);
    return res.data;
  },

  async updateTask(id: string, data: UpdateTaskRequest): Promise<ApiResponse<TaskDto>> {
    const res = await apiClient.put<ApiResponse<TaskDto>>(`/tasks/${id}`, data);
    return res.data;
  },

  async updateTaskStatus(id: string, status: string): Promise<ApiResponse<TaskDto>> {
    const res = await apiClient.patch<ApiResponse<TaskDto>>(`/tasks/${id}/status`, null, { params: { status } });
    return res.data;
  },

  async deleteTask(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/tasks/${id}`);
    return res.data;
  },
};
