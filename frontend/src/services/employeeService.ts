import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';

export interface EmployeeSimpleDto {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  departmentName?: string;
  teamName?: string;
  isActive: boolean;
}

export const employeeService = {
  async getEmployees(): Promise<ApiResponse<EmployeeSimpleDto[]>> {
    const res = await apiClient.get<ApiResponse<EmployeeSimpleDto[]>>('/employees');
    return res.data;
  },
};
