import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type { TimelineEventDto } from '../types/crm';

export const timelineService = {
  async getEntityTimeline(entityType: string, entityId: string, limit: number = 50): Promise<ApiResponse<TimelineEventDto[]>> {
    const res = await apiClient.get<ApiResponse<TimelineEventDto[]>>(`/timeline/${entityType}/${entityId}`, {
      params: { limit },
    });
    return res.data;
  },

  async getRecentTimeline(limit: number = 20): Promise<ApiResponse<TimelineEventDto[]>> {
    const res = await apiClient.get<ApiResponse<TimelineEventDto[]>>('/timeline/recent', {
      params: { limit },
    });
    return res.data;
  },
};
