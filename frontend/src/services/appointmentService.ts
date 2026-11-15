import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  AppointmentDto,
  CreateAppointmentRequest,
  UpdateAppointmentRequest,
} from '../types/crm';

export const appointmentService = {
  async getAppointments(params?: {
    startDate?: string;
    endDate?: string;
    employeeId?: string;
    dealId?: string;
    accountId?: string;
    contactId?: string;
    leadId?: string;
    status?: string;
  }): Promise<ApiResponse<AppointmentDto[]>> {
    const res = await apiClient.get<ApiResponse<AppointmentDto[]>>('/appointments', { params });
    return res.data;
  },

  async getAppointmentById(id: string): Promise<ApiResponse<AppointmentDto>> {
    const res = await apiClient.get<ApiResponse<AppointmentDto>>(`/appointments/${id}`);
    return res.data;
  },

  async createAppointment(data: CreateAppointmentRequest): Promise<ApiResponse<AppointmentDto>> {
    const res = await apiClient.post<ApiResponse<AppointmentDto>>('/appointments', data);
    return res.data;
  },

  async updateAppointment(id: string, data: UpdateAppointmentRequest): Promise<ApiResponse<AppointmentDto>> {
    const res = await apiClient.put<ApiResponse<AppointmentDto>>(`/appointments/${id}`, data);
    return res.data;
  },

  async updateAppointmentStatus(id: string, status: string): Promise<ApiResponse<AppointmentDto>> {
    const res = await apiClient.patch<ApiResponse<AppointmentDto>>(`/appointments/${id}/status`, null, { params: { status } });
    return res.data;
  },

  async deleteAppointment(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/appointments/${id}`);
    return res.data;
  },
};
