import { apiClient } from './apiClient';
import type { ApiResponse, SystemHealthDto, DatabaseHealthDto } from '@/types/api';

export const healthService = {
  getSystemHealth: async (): Promise<SystemHealthDto> => {
    const response = await apiClient.get<ApiResponse<SystemHealthDto>>('/health');
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || 'Failed to retrieve system health.');
    }
    return response.data.data;
  },

  getDatabaseHealth: async (): Promise<DatabaseHealthDto> => {
    const response = await apiClient.get<ApiResponse<DatabaseHealthDto>>('/health/database');
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || 'Database health check failed.');
    }
    return response.data.data;
  },
};
